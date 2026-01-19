namespace Tuuuur.Domain.Bo;

public class PartyQuestion : IBOEntity
{
    public int Id { get; set; }

    public int IdQuestion { get; set; }

    public Guid IdParty { get; set; }

    public int Order { get; set; }

    public PartyBase Party { get; set; }

    public Question Question { get; set; }

    public UserPartyQuestion UserPartyQuestion { get; set; }
}
