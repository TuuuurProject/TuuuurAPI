namespace Tuuuur.Domain.Bo;

public record PartyType : IBOEntity
{
    public int Id { get; set; }

    public string Label { get; set; }
}