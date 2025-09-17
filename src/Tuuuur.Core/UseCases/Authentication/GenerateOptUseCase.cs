using System.Security.Cryptography;
using MediatR;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Configuration;
using Tuuuur.Core.Requests.Authentication;
using Tuuuur.Core.Responses.Authentication;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Emails.Models;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Interfaces.Data.Entities;
using Tuuuur.Domain.Interfaces.Emails;

namespace Tuuuur.Core.UseCases.Authentication;

internal class GenerateOptUseCase(
    IUnitOfWork p_UnitOfWork, 
    ILogger<GenerateOptUseCase> p_Logger) : IRequestHandler<GenerateOptRequest, UserAuthResponse>
{
    [SuppressMessage("Style", "IDE1006:Styles d'affectation de noms", Justification = "Inherited named")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public async Task<UserAuthResponse> Handle(GenerateOptRequest request, CancellationToken cancellationToken)
    {
        try
        {
            int number;
            using (RandomNumberGenerator v_Generator = RandomNumberGenerator.Create())
            {
                byte[] bytes = new byte[4];
                v_Generator.GetBytes(bytes);
                number = BitConverter.ToInt32(bytes, 0) & 0x7FFFFFFF;
            }
            string code = (number % 1000000).ToString("D6");
            
            UserAuth v_Auth = new(){ UserId = request.User.Id, Code = code };
            
            IMappingAddEntity<UserAuth, IEntity> v_UserAuth = await p_UnitOfWork.UserAuthRepository.AddAuthCodeAsync(v_Auth, cancellationToken);
            p_UnitOfWork.Save();
            
            return new UserAuthResponse(v_UserAuth.MapBoEntity);
        }
        catch (Exception v_Ex)
        {
            p_Logger.LogError(v_Ex, "An error was thrown");
            return new UserAuthResponse([v_Ex.ToError()]);
        }
    }
}