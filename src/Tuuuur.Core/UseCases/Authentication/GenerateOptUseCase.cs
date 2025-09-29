using System.Security.Cryptography;
using MediatR;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Configuration;
using Tuuuur.Core.Requests.Authentication;
using Tuuuur.Core.Responses;
using Tuuuur.Core.Responses.Authentication;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Emails.Models;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Interfaces.Data.Entities;
using Tuuuur.Domain.Interfaces.Emails;

namespace Tuuuur.Core.UseCases.Authentication;

internal class GenerateOptUseCase(
    IUnitOfWork p_UnitOfWork, 
    ILogger<GenerateOptUseCase> p_Logger) : IRequestHandler<GenerateOptRequest, GenericEntityResponse<UserAuth>>
{
    [SuppressMessage("Style", "IDE1006:Styles d'affectation de noms", Justification = "Inherited named")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public async Task<GenericEntityResponse<UserAuth>> Handle(GenerateOptRequest request, CancellationToken cancellationToken)
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

            // Needed to avoid spam of 2FA
            long v_CountOfUserExistingAuthsByUserId = await p_UnitOfWork.UserAuthRepository.CountOfUserAuthsByUserIdAsync(request.User.Id, cancellationToken);
            if (v_CountOfUserExistingAuthsByUserId >= 3)
                throw new InvalidOperationException();
            
            UserAuth v_Auth = new() { UserId = request.User.Id, Code = code };

            IMappingAddEntity<UserAuth, IEntity> v_UserAuth =
                await p_UnitOfWork.UserAuthRepository.AddAuthCodeAsync(v_Auth, cancellationToken);
            p_UnitOfWork.Save();

            return new GenericEntityResponse<UserAuth>(v_UserAuth.MapBoEntity);
        }
        catch (InvalidOperationException v_Ex)
        {
            p_Logger.LogError(v_Ex, "The user has too many two-factor authentication (2FA) requests");
            return new GenericEntityResponse<UserAuth>([v_Ex.ToError(DomainErrors.Authentication.Code.TooMuchDemand)]);
        }
        catch (Exception v_Ex)
        {
            p_Logger.LogError(v_Ex, "An error was thrown");
            return new GenericEntityResponse<UserAuth>([v_Ex.ToError()]);
        }
    }
}