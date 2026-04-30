namespace Tuuuur.Domain.Bo;

public record HistoryPage : IBOEntity
{
    public IEnumerable<History> History { get; set; } = [];
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalParties { get; set; }
}
