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
    ILogger<GenerateOptUseCase> p_Logger) : ADbUseCase<GenerateOptRequest, GenericEntityResponse<UserAuth>>(p_Logger, p_UnitOfWork)
{
    protected override async Task<GenericEntityResponse<UserAuth>> HandleLogic(GenerateOptRequest p_Request, CancellationToken p_CancellationToken)
    {
        int v_Number;
        using (RandomNumberGenerator v_Generator = RandomNumberGenerator.Create())
        {
            byte[] v_Bytes = new byte[4];
            v_Generator.GetBytes(v_Bytes);
            v_Number = BitConverter.ToInt32(v_Bytes, 0) & 0x7FFFFFFF;
        }

        string v_Code = (v_Number % 1000000).ToString("D6");

        // Needed to avoid spam of 2FA
        long v_CountOfUserExistingAuthsByUserId = await m_UnitOfWork.UserAuthRepository.CountOfUserAuthsByUserIdAsync(p_Request.User.Id, p_CancellationToken);
        if (v_CountOfUserExistingAuthsByUserId >= 3)
        {
            const string v_Description = "The user has too many two-factor authentication (2FA) requests";
            p_Logger.LogError(v_Description);
            return new GenericEntityResponse<UserAuth>([new ErrorDto(DomainErrors.Authentication.Code.TooMuchDemand, v_Description)]);
        }
            
        UserAuth v_Auth = new() { UserId = p_Request.User.Id, Code = v_Code };

        IMappingAddEntity<UserAuth, IEntity> v_UserAuth =
            await m_UnitOfWork.UserAuthRepository.AddAuthCodeAsync(v_Auth, p_CancellationToken);
        m_UnitOfWork.Save();

        return new GenericEntityResponse<UserAuth>(v_UserAuth.MapBoEntity);    }
}