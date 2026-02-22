namespace Tuuuur.Domain.Bo;

public class PartyUser : IBOEntity
{
    public int Id { get; set; }

    public Guid IdUser { get; set; }

    public Guid IdParty { get; set; }

    public Party Party { get; set; }

    public User User { get; set; }
}
