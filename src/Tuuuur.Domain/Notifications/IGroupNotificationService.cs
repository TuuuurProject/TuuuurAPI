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
    /// <param name="p_PartyId"></param>
    /// <param name="p_User"></param>
    /// <returns></returns>
    Task NotifyPartyDeletedAsync(Guid p_PartyId, User p_User);
    
    /// <summary>
    /// Notify party members that the party was updated
    /// </summary>
    /// <param name="p_PartyId"></param>
    /// <param name="p_Party"></param>
    /// <returns></returns>
    Task NotifyPartyUpdatedAsync(Guid p_PartyId, GroupParty p_Party);
    
    /// <summary>
    /// Notify party members that the party was started
    /// </summary>
    /// <param name="p_PartyId"></param>
    /// <param name="p_Party"></param>
    /// <returns></returns>
    Task NotifyPartyStartedAsync(Guid p_PartyId, GroupParty p_Party);
    
    /// <summary>
    /// Notify party members that a question arrived
    /// </summary>
    /// <param name="p_UserId"></param>
    /// <param name="p_Question"></param>
    /// <returns></returns>
    Task NotifyPartyQuestionSend(int p_UserId, GroupQuestion p_Question);
    
    /// <summary>
    /// Notify user the question with current answer
    /// </summary>
    /// <param name="p_UserId"></param>
    /// <param name="p_Question"></param>
    /// <returns></returns>
    Task NotifyPartyQuestionAnswerSend(int p_UserId, GroupQuestion p_Question);
    
    /// <summary>
    /// Notify user that another user answer the question
    /// </summary>
    /// <param name="p_PartyId"></param>
    /// <param name="p_User"></param>
    /// <returns></returns>
    Task NotifyUserSendAnswerAsync(Guid p_PartyId, User p_User);
    
    /// <summary>
    /// Notify party members the current scores
    /// </summary>
    /// <param name="p_PartyId"></param>
    /// <param name="p_UserScores"></param>
    /// <returns></returns>
    Task NotifyPartyScoresAsync(Guid p_PartyId, IEnumerable<UserScore> p_UserScores);
    
    /// <summary>
    /// Notify party members that the party is finished
    /// </summary>
    /// <param name="p_PartyId"></param>
    /// <param name="p_UserScores"></param>
    /// <returns></returns>
    Task NotifyPartyFinishedAsync(Guid p_PartyId, IEnumerable<UserScore> p_UserScores);

    /// <summary>
    /// Notify party members with countdown value
    /// </summary>
    Task NotifyCountdownAsync(Guid p_PartyId, int p_Seconds);
}
