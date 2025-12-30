using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Authentication;
using Tuuuur.Core.Requests.Tools;
using Tuuuur.Core.Responses;
using Tuuuur.Core.Responses.Authentication;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Interfaces.Token;
using Tuuuur.Domain.Token;

namespace Tuuuur.Core.UseCases.Authentication;

internal class VerifyAccountUseCase(
    IUnitOfWork p_UnitOfWork,
    ILogger<VerifyAccountUseCase> p_Logger,
    IJwtFactory p_JwtFactory)
    : ADbUseCase<VerifyAccountRequest, JwtAuthenticationResponse>(p_Logger, p_UnitOfWork)
{
    protected override async Task<JwtAuthenticationResponse> HandleLogic(VerifyAccountRequest p_Request, CancellationToken p_CancellationToken)
    {
        User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailOrNickNameAsync(p_Request.Login, p_CancellationToken);

        if (v_User == null)
            return new JwtAuthenticationResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {p_Request.Login}")]);

        UserAuth v_UserAuth = await m_UnitOfWork.UserAuthRepository.GetUserAuthByUserIdAndCodeAsync(v_User.Id, p_Request.Code, p_CancellationToken);

        if (v_UserAuth == null)
            return new JwtAuthenticationResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(UserAuth)} was not found, Key: {p_Request.Code}")]);

        v_User.IsNew = false;
        await m_UnitOfWork.UserRepository.UpdateUserAsync(v_User, p_CancellationToken);
        await m_UnitOfWork.UserAuthRepository.DeleteUserAuthAsync(v_UserAuth.Id, p_CancellationToken);

        JwtTokenResponse v_TokenInfos = await p_JwtFactory.CreateTokenAsync(v_User, m_UnitOfWork, p_CancellationToken);
        _ = m_UnitOfWork.Save();

        return new JwtAuthenticationResponse(new UserToken
        {
            Token = v_TokenInfos,
            User = v_User,
            IsGoogleUser = false
        });
    }
}