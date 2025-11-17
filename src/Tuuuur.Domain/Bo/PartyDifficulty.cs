namespace Tuuuur.Domain.Bo;

public record PartyDifficulty
{
    public int Id { get; set; }

    public Guid IdParty { get; set; }
    public int IdDifficulty { get; set; }
    
    public Difficulty Difficulty { get; set; }
}
