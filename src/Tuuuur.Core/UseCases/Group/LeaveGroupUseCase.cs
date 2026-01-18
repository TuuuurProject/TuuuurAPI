using MediatR;
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

internal class LeaveGroupUseCase(IUnitOfWork p_UnitOfWork,
    ILogger<LeaveGroupUseCase> p_Logger,
    IUserRoleService p_UserRoleService,
    IGroupNotificationService p_GroupNotificationService,
    ICacheService p_CacheService) :
    ADbUseCase<LeaveGroupPartyRequest, EmptyResponse>(p_Logger, p_UnitOfWork)
{
    protected override async Task<EmptyResponse> HandleLogic(LeaveGroupPartyRequest p_Request, CancellationToken p_CancellationToken)
    {
        string v_UserEmail = p_UserRoleService.GetCurrentUserEmail();

        User v_User = await m_UnitOfWork.UserRepository.GetUserByEmailAsync(v_UserEmail, p_CancellationToken);

        if (v_User == null)
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(User)} was not found, Key: {v_UserEmail}")]);

        string v_PartyCode = await p_CacheService.GetAsync<string>(RedisKeys.User.UserParty(v_User.Id), p_CancellationToken);

        if (v_PartyCode is null)
        {
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(Party)} was not found")]);
        }

        GroupParty v_Party = await p_CacheService.GetAsync<GroupParty>(RedisKeys.Party.ByCode(v_PartyCode), p_CancellationToken);

        List<int> v_UserInParty = await p_CacheService.SetMembersAsync<int>(RedisKeys.Party.Users(v_Party.Code), p_CancellationToken: p_CancellationToken);

        // If user is not in the party
        if (v_PartyCode != v_Party.Code)
            return new EmptyResponse([new ErrorDto(DomainErrors.Data.NotFound, $"Queried object {nameof(Party)} was not found")]);

        // If the host user leave the party, destroy the party
        if (v_Party.IdUserHost == v_User.Id)
        {
            // Notify all players via WebSocket that host left (party destroyed)
            await p_GroupNotificationService.NotifyPartyDeletedAsync(
                v_Party.Code,
                v_User
            );

            // Send notification to other users
            foreach (int v_UserIdToNotif in v_UserInParty.Where(p_P => p_P != v_User.Id))
            {
                await p_CacheService.RemoveAsync(RedisKeys.User.UserParty(v_UserIdToNotif), p_CancellationToken: p_CancellationToken);
            }

            await p_CacheService.RemoveAsync(RedisKeys.Party.ByCode(v_Party.Code), p_CancellationToken: p_CancellationToken);
            await p_CacheService.RemoveAsync(RedisKeys.Party.Users(v_Party.Code), p_CancellationToken: p_CancellationToken);
        }
        else
        {
            // Notify all players via WebSocket that a player left
            await p_GroupNotificationService.NotifyPlayerLeftAsync(
                v_Party.Code,
                v_User
            );

            await p_CacheService.SetRemoveAsync(RedisKeys.Party.Users(v_Party.Code), v_User.Id, p_CancellationToken: p_CancellationToken);
        }

        await p_CacheService.RemoveAsync(RedisKeys.User.UserParty(v_User.Id), p_CancellationToken: p_CancellationToken);

        return new EmptyResponse();
    }
}