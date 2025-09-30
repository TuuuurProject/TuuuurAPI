namespace Tuuuur.Domain.Bo;

public class PartyUser : IBOEntity
{
    public int Id { get; set; }

    public int IdUser { get; set; }

    public Guid IdParty { get; set; }

    public virtual Party Party { get; set; }

    public virtual User User { get; set; }
}
