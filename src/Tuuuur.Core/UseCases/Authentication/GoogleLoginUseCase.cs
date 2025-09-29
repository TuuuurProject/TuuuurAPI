using System.Text.RegularExpressions;
using Tuuuur.Core.Responses;
using Tuuuur.Core.Responses.Authentication;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data;
using MediatR;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Authentication.Google;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces.Token;
using Tuuuur.Domain.Token;

namespace Tuuuur.Core.UseCases.Authentication;

internal partial class GoogleLoginUseCase(
    IUnitOfWork p_UnitOfWork, 
    ILogger<LoginUseCase> p_Logger,
    IMediator p_Mediator,
    IJwtFactory p_JwtFactory)
    : AUseCase(p_UnitOfWork, p_Logger), IRequestHandler<GoogleLoginRequest, JwtAuthenticationResponse>
{

    [SuppressMessage("Style", "IDE1006:Styles d'affectation de noms", Justification = "Inherited named")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public async Task<JwtAuthenticationResponse> Handle(GoogleLoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(request.Email, cancellationToken);

            if (v_User is null)
            {
                string v_EmailPrefix = request.Email.Split('@')[0];
                v_EmailPrefix = NonAlphanumericRegex().Replace(v_EmailPrefix, "");

                Random v_Random = new Random();
                int v_RandomNumbers = v_Random.Next(10000, 99999);
                v_User = new User
                {
                    Email = request.Email,
                    NickName = $"{v_EmailPrefix}{v_RandomNumbers}",
                    Password = null,
                    IsGoogleUser = true,
                    IsNew = false,
                };
                
                await m_UnitOfWork.UserRepository.CreateUserAsync(v_User, cancellationToken);
                _ = m_UnitOfWork.Save();
            }
            JwtTokenResponse v_TokenInfos = p_JwtFactory.CreateToken(v_User);

            return new JwtAuthenticationResponse(new UserToken
            {
                Token = v_TokenInfos,
                User = v_User
            });
        }
        catch (Exception v_Ex)
        {
            m_Logger.LogError(v_Ex, "An error was thrown");
            return new JwtAuthenticationResponse([v_Ex.ToError()]);
        }
    }

    [GeneratedRegex("[^a-zA-Z0-9]")]
    private static partial Regex NonAlphanumericRegex();
}