namespace Tuuuur.Domain.Bo;

public record Theme : IBOEntity
{
    public int Id { get; set; }
    
    public string Icon { get; set; }
    
    public string Label { get; set; }
}