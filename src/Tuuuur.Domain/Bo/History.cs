namespace Tuuuur.Domain.Bo;

public record History : IBOEntity
{
    public IEnumerable<PartyBase> Parties { get; set; } = [];
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalParties { get; set; }
}
