using Tuuuur.Domain.Bo;

namespace Tuuuur.Domain.Configuration;

/// <summary>
/// Configuration for the Tier/Division/Pool ranking system.
/// Bound from the <c>RankConfiguration</c> section of <c>appsettings.json</c>.
/// </summary>
public class RankConfiguration : IServiceConfiguration
{
    private const string SectionName = "RankConfiguration";

    /// <summary>All question pools, one per Tier.</summary>
    public List<RankPoolConfiguration> Pools { get; set; } = [];

    /// <summary>
    /// ELO thresholds ordered from lowest to highest <see cref="RankThresholdConfiguration.MinElo"/>.
    /// The highest threshold whose <c>MinElo</c> is ≤ the player's ELO determines their rank.
    /// </summary>
    public List<RankThresholdConfiguration> Thresholds { get; set; } = [];

    /// <summary>
    /// Returns the Tier and Division for a given ELO value by finding the highest
    /// threshold whose <see cref="RankThresholdConfiguration.MinElo"/> is ≤ <paramref name="p_Elo"/>.
    /// </summary>
    /// <param name="p_Elo">The player's current ELO (GlobalElo).</param>
    /// <returns>A tuple of (Tier, Division). Division is 0 for the top rank (Tier 8).</returns>
    public (int Tier, int Division) GetRankForElo(int p_Elo)
    {
        if (Thresholds.Count == 0)
            return (1, 3);

        // Find the highest threshold whose MinElo is <= the player's ELO
        RankThresholdConfiguration v_Match = Thresholds
            .Where(p_T => p_T.MinElo <= p_Elo)
            .OrderByDescending(p_T => p_T.MinElo)
            .FirstOrDefault() ?? Thresholds[0];

        return (v_Match.Tier, v_Match.Division);
    }

    /// <summary>
    /// Returns the question pool for a given Tier.
    /// Falls back to the pool with the lowest Tier if no exact match is found.
    /// </summary>
    /// <param name="p_Tier">Target Tier (1–8).</param>
    public RankPoolConfiguration GetPoolForTier(int p_Tier)
    {
        return Pools.FirstOrDefault(p_P => p_P.Tier == p_Tier)
               ?? Pools.OrderBy(p_P => p_P.Tier).FirstOrDefault();
    }

    public string GetSectionName() => SectionName;
}
