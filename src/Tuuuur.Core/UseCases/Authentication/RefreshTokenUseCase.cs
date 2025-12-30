using MediatR;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Authentication;
using Tuuuur.Core.Responses;
using Tuuuur.Core.Responses.Authentication;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Interfaces.Token;
using Tuuuur.Domain.Token;

namespace Tuuuur.Core.UseCases.Authentication;

internal class RefreshTokenUseCase(
    IUnitOfWork p_UnitOfWork,
    ILogger<RefreshTokenUseCase> p_Logger,
    IJwtFactory p_JwtFactory)
    : ADbUseCase<RefreshTokenRequest, JwtAuthenticationResponse>(p_Logger, p_UnitOfWork)
{
    protected override async Task<JwtAuthenticationResponse> HandleLogic(RefreshTokenRequest p_Request, CancellationToken p_CancellationToken)
    {
        RefreshToken v_RefreshToken = await m_UnitOfWork.RefreshTokenRepository.GetRefreshTokenByTokenAsync(p_Request.RefreshToken, p_CancellationToken);

        if (v_RefreshToken == null)
            return new JwtAuthenticationResponse([new ErrorDto(DomainErrors.Authentication.RefreshToken.Invalid, "Invalid refresh token")]);

        if (!v_RefreshToken.IsActive)
        {
            string v_ErrorMessage = v_RefreshToken.IsRevoked
                ? "Refresh token has been revoked"
                : "Refresh token has expired";
            return new JwtAuthenticationResponse([new ErrorDto(DomainErrors.Authentication.RefreshToken.Invalid, v_ErrorMessage)]);
        }

        User v_User = await m_UnitOfWork.UserRepository.GetUserByIdAsync(v_RefreshToken.UserId, p_CancellationToken);

        if (v_User == null)
            return new JwtAuthenticationResponse([new ErrorDto(DomainErrors.Data.NotFound, $"User not found")]);

        v_RefreshToken.IsRevoked = true;
        v_RefreshToken.RevokedAt = DateTime.UtcNow;
        await m_UnitOfWork.RefreshTokenRepository.UpdateRefreshTokenAsync(v_RefreshToken, p_CancellationToken);

        JwtTokenResponse v_TokenInfos = await p_JwtFactory.CreateTokenAsync(v_User, m_UnitOfWork, p_CancellationToken);
        _ = m_UnitOfWork.Save();

        return new JwtAuthenticationResponse(new UserToken
        {
            Token = v_TokenInfos,
            User = v_User,
            IsGoogleUser = v_User.IsGoogleUser
        });
    }
}
