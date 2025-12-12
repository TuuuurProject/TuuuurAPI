namespace Tuuuur.Domain.Bo;

public record Party : PartyBase
{
    public string Code { get; set; }

    public int NbQuestions { get; set; }

    public virtual List<PartyUser> PartyUsers { get; set; } = [];
}