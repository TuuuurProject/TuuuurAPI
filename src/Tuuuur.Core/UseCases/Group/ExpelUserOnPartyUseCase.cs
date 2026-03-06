using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Group;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Configuration;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Notifications;
using Tuuuur.Domain.Security;

namespace Tuuuur.Core.UseCases.Group;

internal class ExpelUserOnPartyUseCase(IUnitOfWork p_UnitOfWork,
    ILogger<ExpelUserOnPartyUseCase> p_Logger,
    IUserRoleService p_UserRoleService,
    IGroupNotificationService p_GroupNotificationService,
    ICacheService p_CacheService) :
    ADbUseCase<ExpelUserOnPartyRequest, EmptyResponse>(p_Logger, p_UnitOfWork)
{
    protected override async Task<EmptyResponse> HandleLogic(ExpelUserOnPartyRequest p_Request, CancellationToken p_CancellationToken)
    {
        string v_UserEmail = p_UserRoleService.GetEmail();
        User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_UserEmail, p_CancellationToken);

        if (v_User == null)
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {v_UserEmail}")]);

        string v_PartyCode = await p_CacheService.GetAsync<string>(RedisKeys.User.UserGroup(v_User.Id), p_CancellationToken);
        if (v_PartyCode is null)
        {
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(Party)} was not found")]);
        }
        
        GroupParty v_Party = await p_CacheService.GetAsync<GroupParty>(RedisKeys.Group.ByCode(v_PartyCode), p_CancellationToken);
        
        // If user is not in the party
        if (v_PartyCode != v_Party.Code || v_Party.IdUserHost != v_User.Id)
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(Party)} was not found")]);
        
        // If party in progress, we can't expel user
        if(v_Party.InProgress)
            return new EmptyResponse([new ErrorDto(DomainErrors.Party.InProgress, $"You can't delete an user when the party is in progress")]);
        
        List<User> v_Users = await p_CacheService.SetMembersAsync<User>(
            RedisKeys.Group.Users(v_Party.Code),
            CancellationToken.None
        );
        
        // If the target user is not in the party
        if(v_Users.All(p_P => p_P.Id != p_Request.UserId))
            return new EmptyResponse([new ErrorDto(DomainErrors.Party.User.NotFound, $"Queried object {nameof(User)}, Key: {p_Request.UserId} was not found on object {nameof(PartyBase)}, Key: {v_Party.Code}")]);
        
        User v_TargetUser = v_Users.FirstOrDefault(p_P => p_P.Id == p_Request.UserId);
        if (v_TargetUser == null)
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {p_Request.UserId}")]);
        
        // Notify that target user is expelled
        await p_GroupNotificationService.NotifyPlayerExpelledAsync(
            v_Party.Code,
            v_TargetUser
        );

        await p_CacheService.SetRemoveAsync(RedisKeys.Group.Users(v_Party.Code), v_TargetUser.Id, p_CancellationToken: p_CancellationToken);
        await p_CacheService.RemoveAsync(RedisKeys.User.UserGroup(v_TargetUser.Id), p_CancellationToken: p_CancellationToken);

        return new EmptyResponse();
    }
}