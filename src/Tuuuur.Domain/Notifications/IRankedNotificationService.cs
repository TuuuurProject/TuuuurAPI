using Tuuuur.Domain.Bo;

namespace Tuuuur.Domain.Notifications;

/// <summary>
/// Service for ranked matchmaking hub notifications
/// </summary>
public interface IRankedNotificationService
{
    /// <summary>
    /// Notify party members with countdown value
    /// </summary>
    Task NotifyCountdownAsync(Guid p_Id, int p_Seconds);

    /// <summary>
    /// Notify both players that a match has been found.
    /// Each player receives their opponent's info and the created party ID.
    /// </summary>
    Task NotifyMatchFoundAsync(User p_Player1, User p_Player2, Guid p_PartyId);

    /// <summary>
    /// Notify party members that a question arrived
    /// </summary>
    /// <param name="p_UserId"></param>
    /// <param name="p_Question"></param>
    /// <returns></returns>
    Task NotifyPartyQuestionSend(Guid p_UserId, RankedQuestion p_Question);

    /// <summary>
    /// Notify user the question with current answer
    /// </summary>
    /// <param name="p_UserId"></param>
    /// <param name="p_Question"></param>
    /// <returns></returns>
    Task NotifyQuestionAnswerSend(Guid p_UserId, RankedQuestion p_Question);

    Task NotifyAllPlayerAnswered(Guid p_Id, IEnumerable<UserAnswered> p_UserAnswered);

    Task NotifyPartyScoresAsync(Guid p_Id, IEnumerable<UserScore> p_UserScores);
    Task NotifyUserSendAnswerAsync(Guid p_UserId, User p_User);
    Task NotifyUserWinAsync(Guid p_UserId, int p_Delta);
    Task NotifyUserLooseAsync(Guid p_UserId, int p_Delta);
}
