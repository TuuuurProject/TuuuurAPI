namespace Tuuuur.Domain.Bo;

public record RankingPage : IBOEntity
{
    public IEnumerable<User> Users { get; set; } = [];
    public int UserRanking { get; set; }
    public int UserElo { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalUsers { get; set; }

    /// <summary>
    /// Tier of the currently authenticated user (1–8).
    /// </summary>
    public int UserTier { get; set; }

    /// <summary>
    /// Division of the currently authenticated user (1–3, or 0 for Tier 8).
    /// </summary>
    public int UserDivision { get; set; }
}
