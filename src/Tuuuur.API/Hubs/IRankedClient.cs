using Tuuuur.Domain.Bo;

namespace Tuuuur.API.Hubs;

/// <summary>
/// Client-side methods that can be invoked from the server
/// </summary>
public interface IRankedClient
{
    /// <summary>
    /// Notified when an opponent is found
    /// </summary>
    Task OnOpponentFound(User p_User);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_Question"></param>
    /// <returns></returns>
    Task OnQuestionSend(RankedQuestion p_Question);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_Question"></param>
    /// <returns></returns>
    Task OnQuestionAnswerSend(RankedQuestion p_Question);
    
    /// <summary>
    /// Notified when score is updated
    /// </summary>
    /// <param name="p_UserAnswered"></param>
    /// <returns></returns>
    Task OnAllPlayerAnswered(IEnumerable<UserAnswered> p_UserAnswered);
    
    /// <summary>
    /// Notified when score is updated
    /// </summary>
    /// <param name="p_UserScores"></param>
    /// <returns></returns>
    Task OnScoreUpdate(IEnumerable<UserScore> p_UserScores);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_User"></param>
    /// <returns></returns>
    Task OnUserAnswer(User p_User); 
    
    /// <summary>
    /// Notified when party is finished
    /// </summary>
    /// <param name="p_UserScores"></param>
    /// <returns></returns>
    Task OnPartyFinished(IEnumerable<UserScore> p_UserScores);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_User"></param>
    /// <returns></returns>
    Task OnUserForfeited(User p_User); 

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_Delta"></param>
    /// <returns></returns>
    Task OnUserWin(int p_Delta);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="p_Delta"></param>
    /// <returns></returns>
    Task OnUserLoose(int p_Delta);

    /// <summary> 
    /// Join matchmaking list
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "SignalR method name")]
    Task JoinSearchOpponent();
    
    /// <summary>
    /// Leave matchmaking list
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "SignalR method name")]
    Task LeaveSearchOpponent();
    
    /// <summary>
    /// Send answer to question
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "SignalR method name")]
    Task SendAnswer(int p_AnswerId);
    
    /// <summary>
    /// Give up ranked
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "SignalR method name")]
    Task GiveUp();
    
    /// <summary>
    /// Notified with countdown value before question
    /// </summary>
    /// <param name="p_Seconds">Remaining seconds</param>
    /// <returns></returns>
    Task OnCountdown(int p_Seconds);

    /// <summary>
    /// Notified when an error occurs
    /// </summary>
    Task OnError(string p_ErrorMessage);
}
