using System.Text.Json.Serialization;

namespace Tuuuur.Domain.Bo;

public record History : IBOEntity
{
    public Guid Id { get; set; }

    public DateTime Dt { get; set; }
    
    public int IdPartyType { get; set; }
    
    public int IdUserHost { get; set; }
    
    public bool Active { get; set; }
    
    public bool Finish { get; set; }
    
    public double Percent => PartyQuestions.Count != 0 ? ((double)PartyQuestions.Count(p_P => p_P.UserPartyQuestion?.Correct is true) / PartyQuestions.Count * 100) : 0;
    
    public int Score => PartyQuestions.Sum(p_P => p_P.UserPartyQuestion?.Score ?? 0);
    
    public int Time => (int)TimeSpan.FromTicks(
        PartyQuestions
            .Where(p_Question => p_Question.UserPartyQuestion?.DtAnsweredAt != null)
            .Sum(p_Question => (p_Question.UserPartyQuestion.DtAnsweredAt.Value - p_Question.UserPartyQuestion.DtPresentedAt).Ticks)
    ).TotalSeconds;

    public int NbQuestions => PartyQuestions.Count;

    public virtual PartyType PartyType { get; set; }

    public virtual User User { get; set; }

    [JsonIgnore]
    public List<PartyQuestion> PartyQuestions { get; set; } = [];
    
    public List<PartyDifficulty> PartyDifficulty { get; set; } = [];

    public List<PartyTheme> PartyTheme { get; set; } = [];
}

public record HistoryPage : IBOEntity
{
    public IEnumerable<History> History { get; set; } = [];
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalParties { get; set; }
}
