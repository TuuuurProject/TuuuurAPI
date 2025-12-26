using Microsoft.AspNetCore.SignalR;
using Tuuuur.API.Hubs;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Configuration;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Notifications;

namespace Tuuuur.API.Notifications;

/// <summary>
/// Implementation of group party notification service
/// Sends notifications to all users in a party (retrieved from Redis)
/// This works with auto-scaling as it targets specific user IDs
/// </summary>
internal class GroupNotificationService(
    IHubContext<GroupHub, IGroupClient> p_HubContext,
    ICacheService p_CacheService,
    ILogger<GroupNotificationService> p_Logger)
    : IGroupNotificationService
{
    private readonly ILogger<GroupNotificationService> m_Logger = p_Logger;

    public async Task NotifyPlayerJoinedAsync(string p_PartyId, User p_User)
    {
        List<string> v_UserIds = await GetPartyUserIdsAsync(p_PartyId);
        if (v_UserIds.Count != 0)
        {
            v_UserIds.Remove(p_User.Id.ToString());
            await p_HubContext.Clients.Users(v_UserIds).OnPlayerJoined(p_User);
        }
    }

    public async Task NotifyPlayerLeftAsync(string p_PartyId, User p_User)
    {
        List<string> v_UserIds = await GetPartyUserIdsAsync(p_PartyId);
        if (v_UserIds.Count != 0)
        {
            v_UserIds.Remove(p_User.Id.ToString());
            await p_HubContext.Clients.Users(v_UserIds).OnPlayerLeft(p_User);
        }
    }

    public async Task NotifyPartyDeletedAsync(string p_PartyId, User p_User)
    {
        List<string> v_UserIds = await GetPartyUserIdsAsync(p_PartyId);
        if (v_UserIds.Count != 0)
        {
            v_UserIds.Remove(p_User.Id.ToString());
            await p_HubContext.Clients.Users(v_UserIds).OnPartyDeleted(p_User);
        }
    }

    public async Task NotifyPartyUpdatedAsync(string p_PartyId, Party p_Party)
    {
        List<string> v_UserIds = await GetPartyUserIdsAsync(p_PartyId);
        if (v_UserIds.Count != 0)
        {
            await p_HubContext.Clients.Users(v_UserIds).OnPartyUpdated(p_Party);
        }
    }

    private async Task<List<string>> GetPartyUserIdsAsync(string p_PartyId)
    {
        // Get user IDs from Redis set
        List<int> v_UserIds = await p_CacheService.SetMembersAsync<int>(
            RedisKeys.Party.Users(p_PartyId),
            CancellationToken.None
        );

        // Convert to string array for SignalR
        return v_UserIds.Select(p_Id => p_Id.ToString()).ToList();
    }
}
