namespace Tuuuur.Domain.Bo;

public class UserPartyQuestion : IBOEntity
{
    public int Id { get; set; }

    public int IdPartyQuestion { get; set; }

    public int IdUser { get; set; }

    public DateTime DtPresentedAt { get; set; }

    public DateTime? DtAnsweredAt { get; set; }
    
    public int? IdAnswer { get; set; }

    public bool? Correct { get; set; }
    
    public int Score { get; set; }
    
    public virtual Answer Answer { get; set; }

    public virtual PartyQuestion PartyQuestion { get; set; }

    public virtual User User { get; set; }
}
