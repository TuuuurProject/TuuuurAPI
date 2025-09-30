namespace Tuuuur.Domain.Bo;

public class UserPartyQuestion
{
    public int Id { get; set; }

    public int IdPartyQuestion { get; set; }

    public int IdUser { get; set; }

    public DateTime Dt { get; set; }

    public bool Correct { get; set; }

    public virtual PartyQuestion PartyQuestion { get; set; }

    public virtual User User { get; set; }
}
