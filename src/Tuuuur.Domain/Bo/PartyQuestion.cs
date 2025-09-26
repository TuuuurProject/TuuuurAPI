namespace Tuuuur.Domain.Bo;

public class PartyQuestion : IBOEntity
{
    public int Id { get; set; }

    public int IdQuestion { get; set; }

    public Guid IdParty { get; set; }

    public int Order { get; set; }

    public Party Party { get; set; }

    public Question Question { get; set; }

    public List<UserPartyQuestion> UserPartyQuestion { get; set; } = [];
}
