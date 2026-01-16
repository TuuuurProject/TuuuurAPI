using Tuuuur.Domain.Bo;

namespace Tuuuur.API.Hubs;

/// <summary>
/// Client-side methods that can be invoked from the server
/// </summary>
public interface IGroupClient
{
    /// <summary>
    /// Notified when a player joins the party
    /// </summary>
    Task OnPlayerJoined(User p_User);

    /// <summary>
    /// Notified when a player leaves the party
    /// </summary>
    Task OnPlayerLeft(User p_User);

    /// <summary>
    /// Notified when the party is deleted
    /// </summary>
    /// <param name="p_User"></param>
    /// <returns></returns>
    Task OnPartyDeleted(User p_User);

    /// <summary>
    /// Notified when the party is updated
    /// </summary>
    /// <param name="p_Party"></param>
    /// <returns></returns>
    Task OnPartyUpdated(GroupParty p_Party);

    /// <summary>
    /// Notified when the party is started
    /// </summary>
    /// <param name="p_Party"></param>
    /// <returns></returns>
    Task OnPartyStarted(GroupParty p_Party);

    /// <summary>
    /// Notified when question was sent
    /// </summary>
    /// <param name="p_Question"></param>
    /// <returns></returns>
    Task OnQuestionSend(GroupQuestion p_Question);
    
    /// <summary>
    /// Notified when question with correct answers was sent
    /// </summary>
    /// <param name="p_Question"></param>
    /// <returns></returns>
    Task OnQuestionAnswerSend(GroupQuestion p_Question);

    /// <summary>
    /// Notified with countdown value before question
    /// </summary>
    /// <param name="p_Seconds">Remaining seconds</param>
    /// <returns></returns>
    Task OnCountdown(int p_Seconds);
    
    /// <summary>
    /// Notified when user answer question
    /// </summary>
    /// <param name="p_User"></param>
    /// <returns></returns>
    Task OnUserAnswer(User p_User);

    /// <summary>
    /// Notified when score is updated
    /// </summary>
    /// <param name="p_UserScores"></param>
    /// <returns></returns>
    Task OnScoreUpdate(IEnumerable<UserScore> p_UserScores);
    
    /// <summary>
    /// Notified when party is finished
    /// </summary>
    /// <param name="p_UserScores"></param>
    /// <returns></returns>
    Task OnPartyFinished(IEnumerable<UserScore> p_UserScores);

    /// <summary>
    /// Notified when the party starts
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "SignalR method name")]
    Task StartGroupParty();
    
    /// <summary>
    /// Send answer to question
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "SignalR method name")]
    Task SendAnswer(int p_AnswerId);

    /// <summary>
    /// Notified when an error occurs
    /// </summary>
    Task OnError(string p_ErrorMessage);
}
