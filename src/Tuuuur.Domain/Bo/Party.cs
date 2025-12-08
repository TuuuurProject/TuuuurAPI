namespace Tuuuur.Domain.Bo;

public record Party : IBOEntity
{
    public Guid Id { get; set; }

    public DateTime Dt { get; set; }

    public string Code { get; set; }

    public int IdPartyType { get; set; }

    public int IdUserHost { get; set; }

    public bool Active { get; set; }

    public bool Finish { get; set; }
    
    public double Percent => (double)PartyQuestions.Count(p_P => p_P.UserPartyQuestion?.Correct is true) / PartyQuestions.Count * 100;
    public int Score => PartyQuestions.Sum(p_P => p_P.UserPartyQuestion?.Score ?? 0);

    public int NbQuestions { get; set; }

    public virtual PartyType PartyType { get; set; }

    public virtual User User { get; set; }

    public virtual List<PartyQuestion> PartyQuestions { get; set; } = [];

    public virtual List<PartyUser> PartyUsers { get; set; } = [];

    public virtual List<PartyDifficulty> PartyDifficulty { get; set; } = [];

    public virtual List<PartyTheme> PartyTheme { get; set; } = [];
}