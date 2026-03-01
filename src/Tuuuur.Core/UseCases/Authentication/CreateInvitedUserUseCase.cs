using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Authentication;
using Tuuuur.Core.Responses.Authentication;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Configuration;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Interfaces.Token;
using Tuuuur.Domain.Token;

namespace Tuuuur.Core.UseCases.Authentication;

internal class CreateInvitedUserUseCase (
    IUnitOfWork p_UnitOfWork, 
    ILogger<VerifyAccountUseCase> p_Logger, 
    ICacheService p_CacheService,
    IJwtFactory p_JwtFactory)
    : ADbUseCase<CreateInvitedUserRequest, JwtAuthenticationResponse>(p_Logger, p_UnitOfWork)
{
    protected override async Task<JwtAuthenticationResponse> HandleLogic(CreateInvitedUserRequest p_Request, CancellationToken p_CancellationToken)
    {
        User v_User = new()
        { 
            Id = Guid.NewGuid(),
            NickName = p_Request.NickName,
        };
        
        JwtTokenResponse v_TokenInfos = p_JwtFactory.CreateAnonymousTokenAsync(v_User);
        
        // Store it 24 hours in redis
        await p_CacheService.SetAsync(RedisKeys.User.GroupById(v_User.Id), v_User, TimeSpan.FromHours(24), p_CancellationToken: p_CancellationToken);

        return new JwtAuthenticationResponse(new UserToken
        {
            Token = v_TokenInfos,
            User = v_User
        });
    }
}
