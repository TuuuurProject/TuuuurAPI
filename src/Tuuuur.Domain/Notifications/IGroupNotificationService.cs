using Tuuuur.Domain.Bo;

namespace Tuuuur.Domain.Notifications;

/// <summary>
/// Service for group party hub notifications
/// </summary>
public interface IGroupNotificationService
{
    /// <summary>
    /// Notify party members that a player joined
    /// </summary>
    Task NotifyPlayerJoinedAsync(Guid p_PartyId, User p_User);

    /// <summary>
    /// Notify party members that a player left
    /// </summary>
    Task NotifyPlayerLeftAsync(Guid p_PartyId, User p_User);

    /// <summary>
    /// Notify party members that the party was deleted
    /// </summary>
    Task NotifyPartyDeletedAsync(Guid p_PartyId, User p_User);
    Task NotifyPartyUpdatedAsync(Guid p_PartyId, GroupParty p_Party);
    Task NotifyPartyStartedAsync(Guid p_PartyId, GroupParty p_Party);
    Task NotifyPartyQuestionSend(int p_UserId, GroupQuestion p_Question);
    Task NotifyPartyQuestionAnswerSend(int p_UserId, GroupQuestion p_Question);
    Task NotifyUserSendAnswerAsync(Guid p_PartyId, User p_User);
    Task NotifyPartyScoresAsync(Guid p_PartyId, IEnumerable<UserScore> p_UserScores);

    /// <summary>
    /// Notify party members with countdown value
    /// </summary>
    Task NotifyCountdownAsync(Guid p_PartyId, int p_Seconds);
}
