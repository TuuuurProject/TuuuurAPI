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
    /// K-factor tiers, ordered by ascending MaxElo.
    /// Default: K=40 up to 1200, K=20 up to 2000, K=10 above.
    /// </summary>
    public List<KFactorThreshold> KFactorThresholds { get; set; } =
    [
        new KFactorThreshold { MaxElo = 1200, K = 40 },
        new KFactorThreshold { MaxElo = 2000, K = 20 },
        new KFactorThreshold { MaxElo = int.MaxValue, K = 10 },
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
