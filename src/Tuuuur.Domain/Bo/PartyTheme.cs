namespace Tuuuur.Domain.Bo;

public class PartyTheme : IBOEntity
{
    public int Id { get; set; }

    public Guid IdParty { get; set; }

    public int IdTheme { get; set; }
    
    public virtual Party Party { get; set; }

    public virtual Theme Theme { get; set; }
}
