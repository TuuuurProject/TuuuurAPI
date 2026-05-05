using Microsoft.AspNetCore.SignalR;
using Tuuuur.API.Hubs;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Configuration;
using Tuuuur.Domain.Interfaces;
using Tuuuur.Domain.Notifications;

namespace Tuuuur.API.Notifications;

/// <summary>
/// Implementation of ranked notification service.
/// Sends SignalR notifications to individual players by their user ID.
/// Works with auto-scaling because it targets specific user IDs (not group connections).
/// </summary>
internal class RankedNotificationService(
    IHubContext<RankedHub, IRankedClient> p_HubContext,
    ICacheService p_CacheService,
    ILogger<RankedNotificationService> p_Logger)
    : IRankedNotificationService
{
    private readonly ILogger<RankedNotificationService> m_Logger = p_Logger;

    private async Task<List<string>> GetPartyUserIdsAsync(Guid p_Id)
    {
        Party v_Party = await p_CacheService.GetAsync<Party>(
            RedisKeys.Ranked.ById(p_Id),
            CancellationToken.None
        );

        return v_Party.PartyUsers.Select(p_P => p_P.User.Id.ToString()).ToList();
    }

    public async Task NotifyCountdownAsync(Guid p_Id, int p_Seconds)
    {
        List<string> v_UserIds = await GetPartyUserIdsAsync(p_Id);
        if (v_UserIds.Count != 0)
        {
            await p_HubContext.Clients.Users(v_UserIds).OnCountdown(p_Seconds);
        }
    }

    public async Task NotifyMatchFoundAsync(User p_Player1, User p_Player2, Guid p_PartyId)
    {
        m_Logger.LogInformation(
            "Notifying match found: {P1} vs {P2}, PartyId={PartyId}",
            p_Player1.Id, p_Player2.Id, p_PartyId);

        // Each player receives their opponent's User object (the client shows who they will face)
        await Task.WhenAll(
            p_HubContext.Clients.User(p_Player1.Id.ToString()).OnOpponentFound(p_Player2),
            p_HubContext.Clients.User(p_Player2.Id.ToString()).OnOpponentFound(p_Player1)
        );
    }

    public async Task NotifyPartyQuestionSend(Guid p_UserId, RankedQuestion p_Question)
    {
        await p_HubContext.Clients.User(p_UserId.ToString()).OnQuestionSend(p_Question);
    }

    public async Task NotifyQuestionAnswerSend(Guid p_UserId, RankedQuestion p_Question)
    {
        await p_HubContext.Clients.Users(p_UserId.ToString()).OnQuestionAnswerSend(p_Question);
    }


    public async Task NotifyAllPlayerAnswered(Guid p_Id, IEnumerable<UserAnswered> p_UserAnswered)
    {
        List<string> v_UserIds = await GetPartyUserIdsAsync(p_Id);
        if (v_UserIds.Count != 0)
        {
            await p_HubContext.Clients.Users(v_UserIds).OnAllPlayerAnswered(p_UserAnswered);
        }
    }

    public async Task NotifyPlayerForfeited(Guid p_UserId, User p_Player)
    {
        await p_HubContext.Clients.Users(p_UserId.ToString()).OnUserForfeited(p_Player);
    }

    public async Task NotifyPartyScoresAsync(Guid p_Id, IEnumerable<UserScore> p_UserScores)
    {
        List<string> v_UserIds = await GetPartyUserIdsAsync(p_Id);
        if (v_UserIds.Count != 0)
        {
            await p_HubContext.Clients.Users(v_UserIds).OnScoreUpdate(p_UserScores);
        }
    }

    public async Task NotifyUserSendAnswerAsync(Guid p_UserId, User p_User)
    {
        await p_HubContext.Clients.Users(p_UserId.ToString()).OnUserAnswer(p_User);
    }

    public async Task NotifyPartyFinishedAsync(Guid p_Id, IEnumerable<UserScore> p_UserScores)
    {
        List<string> v_UserIds = await GetPartyUserIdsAsync(p_Id);
        if (v_UserIds.Count != 0)
        {
            await p_HubContext.Clients.Users(v_UserIds).OnPartyFinished(p_UserScores);
        }
    }

    public async Task NotifyUserWinAsync(Guid p_UserId, int p_Delta)
    {
        await p_HubContext.Clients.User(p_UserId.ToString()).OnUserWin(p_Delta);
    }

    public async Task NotifyUserLooseAsync(Guid p_UserId, int p_Delta)
    {
        await p_HubContext.Clients.User(p_UserId.ToString()).OnUserLoose(p_Delta);
    }
}
