namespace Tuuuur.Domain.Bo;

public record RankingPage : IBOEntity
{
    public IEnumerable<User> Users { get; set; } = [];
    public int UserRanking { get; set; }
    public int UserElo { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalUsers { get; set; }
}
