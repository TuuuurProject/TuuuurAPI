namespace Tuuuur.Domain.Bo;

public abstract record PartyBase : IBOEntity
{
    public Guid Id { get; set; }
    public DateTime Dt { get; set; }
    public int IdPartyType { get; set; }
    public Guid IdUserHost { get; set; }
    public bool Active { get; set; }
    public bool Finish { get; set; }
    
    public double Percent => PartyQuestions.Count != 0 
        ? ((double)PartyQuestions.Count(p_Question => p_Question.UserPartyQuestion?.Correct is true) / PartyQuestions.Count * 100) 
        : 0;

    public int Score => PartyQuestions.Sum(p_PartyQuestion => p_PartyQuestion.UserPartyQuestion?.Score ?? 0);

    public int Time => (int)TimeSpan.FromTicks(
        PartyQuestions
            .Where(p_Question => p_Question.UserPartyQuestion?.DtAnsweredAt != null)
            .Sum(p_Question => (p_Question.UserPartyQuestion.DtAnsweredAt.Value - p_Question.UserPartyQuestion.DtPresentedAt).Ticks)
    ).TotalSeconds;

    public PartyType PartyType { get; set; }
    public User User { get; set; }
    public List<PartyDifficulty> PartyDifficulty { get; set; } = [];
    public List<PartyTheme> PartyTheme { get; set; } = [];
    public List<PartyQuestion> PartyQuestions { get; set; } = [];
}