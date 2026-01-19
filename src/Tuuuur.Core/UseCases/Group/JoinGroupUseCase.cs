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
using Notification = Tuuuur.Domain.Bo.Enum.Notification;

namespace Tuuuur.Core.UseCases.Group;

internal class JoinGroupUseCase(IUnitOfWork p_UnitOfWork,
    ILogger<JoinGroupUseCase> p_Logger,
    IUserRoleService p_UserRoleService,
    ICacheService p_CacheService,
    IGroupNotificationService p_GroupNotificationService) :
    ACreateJoinGroupUseCase<JoinGroupPartyRequest>(p_Logger, p_UnitOfWork, p_UserRoleService, p_CacheService)
{

    protected override async Task<GenericEntityResponse<GroupParty>> Process(JoinGroupPartyRequest p_Request, User p_User, CancellationToken p_CancellationToken)
    {
        GroupParty v_Party = null;
        if (!string.IsNullOrWhiteSpace(p_Request.Code))
        {
            v_Party = await m_CacheService.GetAsync<GroupParty>(RedisKeys.Party.ByCode(p_Request.Code), p_CancellationToken);
        }

        if (v_Party == null)
            return new GenericEntityResponse<GroupParty>([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(GroupParty)} was not found, Key: {p_Request.Code}")]);

        List<int> v_UserInParty = await m_CacheService.SetMembersAsync<int>(RedisKeys.Party.Users(v_Party.Code), p_CancellationToken: p_CancellationToken);

        await m_CacheService.SetAddAsync(RedisKeys.Party.Users(v_Party.Code), p_User.Id, p_CancellationToken: p_CancellationToken);
        await m_CacheService.SetAsync(RedisKeys.User.UserParty(p_User.Id), v_Party.Code, p_CancellationToken: p_CancellationToken);

        await p_GroupNotificationService.NotifyPlayerJoinedAsync(
            v_Party.Code,
            p_User
        );

        foreach (int v_UserIdToNotif in v_UserInParty)
        {
            User v_UserToNotify = await m_UnitOfWork.UserRepository.GetUserByIdAsync(v_UserIdToNotif, p_CancellationToken);
            v_Party.PartyUsers.Add(new PartyUser { User = v_UserToNotify, IdUser = v_UserIdToNotif });
        }
        v_Party.PartyUsers.Add(new PartyUser { User = p_User, IdUser = p_User.Id });
        return new GenericEntityResponse<GroupParty>(v_Party);
    }
}