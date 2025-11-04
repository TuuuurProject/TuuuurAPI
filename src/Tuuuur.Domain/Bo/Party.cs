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
    
    
    public int Score => PartyQuestions.Sum(p_P => p_P.UserPartyQuestion.Score);
    
    
    public int NbQuestions { get; set; }

    public virtual PartyType PartyType { get; set; }

    public virtual User User { get; set; }

    public virtual List<PartyQuestion> PartyQuestions { get; set; } = [];

    public virtual List<PartyUser> PartyUsers { get; set; } = [];

    public virtual List<PartyDifficulty> PartyDifficulty { get; set; } = [];

    public virtual List<PartyTheme> PartyTheme { get; set; } = [];
}
