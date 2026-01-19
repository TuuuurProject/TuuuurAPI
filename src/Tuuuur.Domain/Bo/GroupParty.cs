namespace Tuuuur.Domain.Bo;

public record GroupParty : PartyBase
{
    /// <summary>
    /// Party code
    /// </summary>
    public string Code { get; set; }
    
    /// <summary>
    /// Is the party in progress (for groups and duel)
    /// </summary>
    public bool InProgress { get; set; }
    
    /// <summary>
    /// Is score is send each time the round is finished
    /// </summary>
    public bool ScoreEachRound  { get; set; }
    
    /// <summary>
    /// Users list
    /// </summary>
    public virtual List<PartyUser> PartyUsers { get; set; } = [];
}