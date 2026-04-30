using System.Text.Json.Serialization;

namespace Tuuuur.Domain.Bo;

public record RankedParty : PartyBase
{
    /// <summary>
    /// Number of questions
    /// </summary>
    public int NbQuestions { get; set; }
    
    /// <summary>
    /// GroupUsers list
    /// </summary>
    public virtual List<PartyUser> PartyUsers { get; set; } = [];
    
    [JsonIgnore]
    public List<UserScore> UserScores { get; set; } = [];
    
    /// <summary>
    /// If the player is winner or not
    /// </summary>
    public bool IsWinner  { get; set; }
    
    /// <summary>
    /// Party elo result
    /// </summary>
    public int Elo  { get; set; }
    
    /// <summary>
    /// Final user score
    /// </summary>
    public int FinalScore  { get; set; }
}