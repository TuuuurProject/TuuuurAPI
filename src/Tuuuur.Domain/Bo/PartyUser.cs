namespace Tuuuur.Domain.Bo;

public class PartyUser : IBOEntity
{
    public int Id { get; set; }

    public int IdUser { get; set; }

    public Guid IdParty { get; set; }

    public PartyBase Party { get; set; }

    public User User { get; set; }
}
