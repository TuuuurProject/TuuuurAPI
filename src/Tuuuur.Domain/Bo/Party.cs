namespace Tuuuur.Domain.Bo;

public record Party : PartyBase
{
    /// <summary>
    /// Number of questions
    /// </summary>
    public int NbQuestions { get; set; }
    
    /// <summary>
    /// Is the party in progress (for groups and duel)
    /// </summary>
    public bool InProgress { get; set; }
    
    /// <summary>
    /// Users list
    /// </summary>
    public List<PartyUser> PartyUsers { get; set; } = [];
    
    public List<UserScore> UserScores { get; set; } = [];
}