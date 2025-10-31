using System.Text.RegularExpressions;
using Ardalis.GuardClauses;
using Tuuuur.Core.Responses.Authentication;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Authentication.Google;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces.Token;
using Tuuuur.Domain.Token;

namespace Tuuuur.Core.UseCases.Authentication;

internal partial class GoogleAuthentificationUseCase(
    IUnitOfWork p_UnitOfWork, 
    ILogger<GoogleAuthentificationUseCase> p_Logger,
    IJwtFactory p_JwtFactory)
    : ADbUseCase<GoogleAuthentificationRequest, JwtAuthenticationResponse>(p_Logger, p_UnitOfWork)
{
    protected override async Task<JwtAuthenticationResponse> HandleLogic(GoogleAuthentificationRequest p_Request, CancellationToken p_CancellationToken)
    {
        User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(p_Request.Email, p_CancellationToken);

        // If the user is null, create it
        if (v_User is null)
        {
            string v_EmailPrefix = p_Request.Email.Split('@')[0];
            v_EmailPrefix = NonAlphanumericRegex().Replace(v_EmailPrefix, "");

            Random v_Random = new Random();
            int v_RandomNumbers = v_Random.Next(10000, 99999);
            v_User = new User
            {
                Email = p_Request.Email,
                NickName = $"{v_EmailPrefix}{v_RandomNumbers}",
                Password = null,
                IsGoogleUser = true,
                IsNew = false,
            };
                
            await m_UnitOfWork.UserRepository.CreateUserAsync(v_User, p_CancellationToken);
            _ = m_UnitOfWork.Save();
        }
        
        else if (!v_User.IsGoogleUser)
            return new JwtAuthenticationResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {p_Request.Email}")]);
            
        JwtTokenResponse v_TokenInfos = p_JwtFactory.CreateToken(v_User);

        return new JwtAuthenticationResponse(new UserToken
        {
            Token = v_TokenInfos,
            User = v_User,
            IsGoogleUser = true
        });
    }

    [GeneratedRegex("[^a-zA-Z0-9]")]
    private static partial Regex NonAlphanumericRegex();
}