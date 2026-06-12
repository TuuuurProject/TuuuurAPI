using Tuuuur.Domain.Configuration;

namespace Tuuuur.Infrastructure.Services;

/// <summary>
/// Variable K-factor thresholds for the Elo calculation.
/// Each entry defines the upper Elo bound (inclusive) for which that K applies.
/// Entries are evaluated in ascending MaxElo order; the last entry acts as a catch-all.
/// </summary>
public class EloConfiguration : IServiceConfiguration
{
    private const string SectionName = "EloConfiguration";

    /// <summary>
    /// Number of games below which the placement K-factor applies.
    /// Default: 20.
    /// </summary>
    public int PlacementGames { get; set; } = 20;

    /// <summary>
    /// K-factor used during the placement phase (GamesPlayed &lt; PlacementGames).
    /// Default: 60 — ensures fast early convergence.
    /// </summary>
    public int PlacementKFactor { get; set; } = 60;

    /// <summary>
    /// K-factor tiers, ordered by ascending MaxElo, applied after the placement phase.
    /// Default: K=40 up to 1500, K=25 up to 2500, K=18 above.
    /// </summary>
    public List<KFactorThreshold> KFactorThresholds { get; set; } =
    [
        new KFactorThreshold { MaxElo = 1500, K = 40 },
        new KFactorThreshold { MaxElo = 2500, K = 25 },
        new KFactorThreshold { MaxElo = int.MaxValue, K = 18 },
    ];

    public string GetSectionName() => SectionName;
}

public class KFactorThreshold
{
    /// <summary>Upper bound (inclusive) of the Elo range this K applies to.</summary>
    public int MaxElo { get; set; }

    /// <summary>K-factor value to use when the player's Elo is ≤ MaxElo.</summary>
    public int K { get; set; }
}