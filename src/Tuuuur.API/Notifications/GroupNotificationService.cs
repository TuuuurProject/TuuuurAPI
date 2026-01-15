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

    public async Task NotifyPlayerJoinedAsync(Guid p_PartyId, User p_User)
    {
        List<string> v_UserIds = await GetPartyUserIdsAsync(p_PartyId);
        if (v_UserIds.Count != 0)
        {
            v_UserIds.Remove(p_User.Id.ToString());
            await p_HubContext.Clients.Users(v_UserIds).OnPlayerJoined(p_User);
        }
    }

    public async Task NotifyPlayerLeftAsync(Guid p_PartyId, User p_User)
    {
        List<string> v_UserIds = await GetPartyUserIdsAsync(p_PartyId);
        if (v_UserIds.Count != 0)
        {
            v_UserIds.Remove(p_User.Id.ToString());
            await p_HubContext.Clients.Users(v_UserIds).OnPlayerLeft(p_User);
        }
    }

    public async Task NotifyPartyDeletedAsync(Guid p_PartyId, User p_User)
    {
        List<string> v_UserIds = await GetPartyUserIdsAsync(p_PartyId);
        if (v_UserIds.Count != 0)
        {
            v_UserIds.Remove(p_User.Id.ToString());
            await p_HubContext.Clients.Users(v_UserIds).OnPartyDeleted(p_User);
        }
    }

    public async Task NotifyPartyUpdatedAsync(Guid p_PartyId, GroupParty p_Party)
    {
        List<string> v_UserIds = await GetPartyUserIdsAsync(p_PartyId);
        if (v_UserIds.Count != 0)
        {
            await p_HubContext.Clients.Users(v_UserIds).OnPartyUpdated(p_Party);
        }
    }

    public async Task NotifyPartyStartedAsync(Guid p_PartyId, GroupParty p_Party)
    {
        List<string> v_UserIds = await GetPartyUserIdsAsync(p_PartyId);
        if (v_UserIds.Count != 0)
        {
            await p_HubContext.Clients.Users(v_UserIds).OnPartyStarted(p_Party);
        }
    }

    public async Task NotifyPartyQuestionSend(int p_UserId, GroupQuestion p_Question)
    {
        await p_HubContext.Clients.User(p_UserId.ToString()).OnQuestionSend(p_Question);
    }
    
    public async Task NotifyPartyQuestionAnswerSend(int p_UserId, GroupQuestion p_Question)
    {
        await p_HubContext.Clients.Users(p_UserId.ToString()).OnQuestionAnswerSend(p_Question);
    }

    public async Task NotifyUserSendAnswerAsync(Guid p_PartyId, User p_User)
    {
        List<string> v_UserIds = await GetPartyUserIdsAsync(p_PartyId);
        if (v_UserIds.Count != 0)
        {
            await p_HubContext.Clients.Users(v_UserIds.Where(p_P => p_P != p_User.Id.ToString())).OnUserAnswer(p_User);
        }
    }

    public async Task NotifyPartyScoresAsync(Guid p_PartyId, IEnumerable<UserScore> p_UserScores)
    {
        List<string> v_UserIds = await GetPartyUserIdsAsync(p_PartyId);
        if (v_UserIds.Count != 0)
        {
            await p_HubContext.Clients.Users(v_UserIds).OnScoreUpdate(p_UserScores);
        }
    }

    public async Task NotifyCountdownAsync(Guid p_PartyId, int p_Seconds)
    {
        List<string> v_UserIds = await GetPartyUserIdsAsync(p_PartyId);
        if (v_UserIds.Count != 0)
        {
            await p_HubContext.Clients.Users(v_UserIds).OnCountdown(p_Seconds);
        }
    }

    private async Task<List<string>> GetPartyUserIdsAsync(Guid p_PartyId)
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
