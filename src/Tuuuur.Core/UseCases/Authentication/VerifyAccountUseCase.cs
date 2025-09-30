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

internal class VerifyAccountUseCase(IUnitOfWork p_UnitOfWork, ILogger<VerifyAccountUseCase> p_Logger, IJwtFactory p_JwtFactory)
    : AUseCase(p_UnitOfWork, p_Logger), IRequestHandler<VerifyAccountRequest, JwtAuthenticationResponse>
{
    [SuppressMessage("Style", "IDE1006:Styles d'affectation de noms", Justification = "Inherited named")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public async Task<JwtAuthenticationResponse> Handle(VerifyAccountRequest request, CancellationToken cancellationToken)
    {
        try
        {
            User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailOrNickNameAsync(request.Login, cancellationToken) 
                ?? throw new NotFoundException(request.Login, nameof(User));
            
            UserAuth v_UserAuth = await m_UnitOfWork.UserAuthRepository.GetUserAuthByUserIdAndCodeAsync(v_User.Id, request.Code, cancellationToken)
                ?? throw new NotFoundException(request.Code, nameof(UserAuth));
            
            v_User.IsNew = false;
            await m_UnitOfWork.UserRepository.UpdateUserAsync(v_User, cancellationToken);
            await m_UnitOfWork.UserAuthRepository.DeleteUserAuthAsync(v_UserAuth.Id, cancellationToken);
            _ = m_UnitOfWork.Save();

            JwtTokenResponse v_TokenInfos = p_JwtFactory.CreateToken(v_User);

            return new JwtAuthenticationResponse(new UserToken
            {
                Token = v_TokenInfos,
                User = v_User,
                IsGoogleUser = false
            });
        }
        catch (NotFoundException v_Ex)
        {
            m_Logger.LogError(v_Ex, "Data not found");
            return new JwtAuthenticationResponse([v_Ex.ToError(DomainErrors.Data.NotFound)]);
        }
        catch (Exception v_Ex)
        {
            m_Logger.LogError(v_Ex, "An error was thrown");
            return new JwtAuthenticationResponse([v_Ex.ToError()]);
        }
    }
}