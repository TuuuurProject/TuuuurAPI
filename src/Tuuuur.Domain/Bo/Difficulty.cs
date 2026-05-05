namespace Tuuuur.Domain.Bo;

public record Difficulty : IBOEntity
{
    public int Id { get; set; }

    public string Label { get; set; }
}