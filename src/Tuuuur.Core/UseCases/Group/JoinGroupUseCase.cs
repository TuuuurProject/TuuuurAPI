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

internal class JoinGroupUseCase(IUnitOfWork p_UnitOfWork,
    ILogger<JoinGroupUseCase> p_Logger,
    IUserRoleService p_UserRoleService,
    ICacheService p_CacheService,
    IGroupNotificationService p_GroupNotificationService) :
    ADbUseCase<JoinGroupPartyRequest, GenericEntityResponse<GroupParty>>(p_Logger, p_UnitOfWork)
{
    protected override async Task<GenericEntityResponse<GroupParty>> HandleLogic(JoinGroupPartyRequest p_Request, CancellationToken p_CancellationToken)
    {
        Guid v_UserId = p_UserRoleService.GetUserId();
        string v_PartyCode = await p_CacheService.GetAsync<string>(RedisKeys.User.UserGroup(v_UserId), p_CancellationToken) ?? string.Empty;
        
        // If user is in party, 
        if (v_PartyCode != string.Empty)
        {
            GroupParty v_ExistingParty = await p_CacheService.GetAsync<GroupParty>(RedisKeys.Group.ByCode(v_PartyCode), p_CancellationToken);
            List<User> v_UserInExistingParty = await p_CacheService.SetMembersAsync<User>(RedisKeys.Group.Users(v_PartyCode), p_CancellationToken: p_CancellationToken);
            foreach (User v_LocalUser in v_UserInExistingParty)
            {
                v_ExistingParty.PartyUsers.Add(new PartyUser { User =  v_LocalUser, IdUser = v_LocalUser.Id });
            }

            return new GenericEntityResponse<GroupParty>(v_ExistingParty);
        }
        
        GroupParty v_Party = null;
        if (!string.IsNullOrWhiteSpace(p_Request.Code))
        {
            v_Party = await p_CacheService.GetAsync<GroupParty>(RedisKeys.Group.ByCode(p_Request.Code), p_CancellationToken);
        }

        if (v_Party == null)
            return new GenericEntityResponse<GroupParty>([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(Party)} was not found, Key: {p_Request.Code}")]);

        List<User> v_UserInParty = await p_CacheService.SetMembersAsync<User>(RedisKeys.Group.Users(v_Party.Code), p_CancellationToken: p_CancellationToken);

        User v_User = await m_UnitOfWork.UserRepository.GetUserByIdAsync(v_UserId, p_CancellationToken) 
                      ?? await p_CacheService.GetAsync<User>(RedisKeys.User.GroupById(v_UserId), p_CancellationToken);
        await p_CacheService.SetAddAsync(RedisKeys.Group.Users(v_Party.Code), v_User, p_CancellationToken: p_CancellationToken);
        await p_CacheService.SetAsync(RedisKeys.User.UserGroup(v_UserId), v_Party.Code, p_CancellationToken: p_CancellationToken);

        await p_GroupNotificationService.NotifyPlayerJoinedAsync(
            v_Party.Code,
            v_User
        );
        
        // Update the user in redis to avoid it to be deleted during party
        await p_CacheService.SetAsync(RedisKeys.User.GroupById(v_User.Id), v_User, TimeSpan.FromHours(24), p_CancellationToken: p_CancellationToken);

        foreach (User v_UserToNotify in v_UserInParty)
        {
            v_Party.PartyUsers.Add(new PartyUser { User = v_UserToNotify, IdUser = v_UserToNotify.Id });
        }
        v_Party.PartyUsers.Add(new PartyUser { User = v_User, IdUser = v_User.Id });
        return new GenericEntityResponse<GroupParty>(v_Party);
    }
}