using Tuuuur.Domain.Bo.Enum;
using Tuuuur.Domain.Configuration;

namespace Tuuuur.Infrastructure.Services;

/// <summary>
/// Configuration that maps Elo ranges to ranks.
/// Each threshold defines the minimum Elo required to reach a given tier + division.
/// Thresholds must be ordered by ascending <see cref="RankThreshold.MinElo"/>.
/// The rank for a given Elo is the highest threshold whose MinElo is ≤ that Elo.
/// </summary>
public class RankConfiguration : IServiceConfiguration
{
    private const string SectionName = "RankConfiguration";

    /// <summary>
    /// Rank thresholds, ordered by ascending MinElo.
    /// Default ranges:
    /// <list type="table">
    ///   <listheader><term>Rank</term><description>Elo range</description></listheader>
    ///   <item><term>Fer 3</term><description>0 – 399</description></item>
    ///   <item><term>Fer 2</term><description>400 – 649</description></item>
    ///   <item><term>Fer 1</term><description>650 – 849 (default start = 800)</description></item>
    ///   <item><term>Bronze 3</term><description>850 – 949</description></item>
    ///   <item><term>Bronze 2</term><description>950 – 1049</description></item>
    ///   <item><term>Bronze 1</term><description>1050 – 1199</description></item>
    ///   <item><term>Argent 3</term><description>1200 – 1349</description></item>
    ///   <item><term>Argent 2</term><description>1350 – 1499</description></item>
    ///   <item><term>Argent 1</term><description>1500 – 1649</description></item>
    ///   <item><term>Or 3</term><description>1650 – 1799</description></item>
    ///   <item><term>Or 2</term><description>1800 – 1949</description></item>
    ///   <item><term>Or 1</term><description>1950 – 2099</description></item>
    ///   <item><term>Platine 3</term><description>2100 – 2249</description></item>
    ///   <item><term>Platine 2</term><description>2250 – 2399</description></item>
    ///   <item><term>Platine 1</term><description>2400 – 2549</description></item>
    ///   <item><term>Diamant 3</term><description>2550 – 2699</description></item>
    ///   <item><term>Diamant 2</term><description>2700 – 2849</description></item>
    ///   <item><term>Diamant 1</term><description>2850 – 2999</description></item>
    ///   <item><term>Maître 3</term><description>3000 – 3199</description></item>
    ///   <item><term>Maître 2</term><description>3200 – 3399</description></item>
    ///   <item><term>Maître 1</term><description>3400 – 3599</description></item>
    ///   <item><term>Champion</term><description>3600+</description></item>
    /// </list>
    /// </summary>
    public List<RankThreshold> Thresholds { get; set; } =
    [
        new RankThreshold { MinElo = 0,    Tier = RankTier.Fer,      Division = RankDivision.Three },
        new RankThreshold { MinElo = 400,  Tier = RankTier.Fer,      Division = RankDivision.Two   },
        new RankThreshold { MinElo = 650,  Tier = RankTier.Fer,      Division = RankDivision.One   },
        new RankThreshold { MinElo = 850,  Tier = RankTier.Bronze,   Division = RankDivision.Three },
        new RankThreshold { MinElo = 950,  Tier = RankTier.Bronze,   Division = RankDivision.Two   },
        new RankThreshold { MinElo = 1050, Tier = RankTier.Bronze,   Division = RankDivision.One   },
        new RankThreshold { MinElo = 1200, Tier = RankTier.Argent,   Division = RankDivision.Three },
        new RankThreshold { MinElo = 1350, Tier = RankTier.Argent,   Division = RankDivision.Two   },
        new RankThreshold { MinElo = 1500, Tier = RankTier.Argent,   Division = RankDivision.One   },
        new RankThreshold { MinElo = 1650, Tier = RankTier.Or,       Division = RankDivision.Three },
        new RankThreshold { MinElo = 1800, Tier = RankTier.Or,       Division = RankDivision.Two   },
        new RankThreshold { MinElo = 1950, Tier = RankTier.Or,       Division = RankDivision.One   },
        new RankThreshold { MinElo = 2100, Tier = RankTier.Platine,  Division = RankDivision.Three },
        new RankThreshold { MinElo = 2250, Tier = RankTier.Platine,  Division = RankDivision.Two   },
        new RankThreshold { MinElo = 2400, Tier = RankTier.Platine,  Division = RankDivision.One   },
        new RankThreshold { MinElo = 2550, Tier = RankTier.Diamant,  Division = RankDivision.Three },
        new RankThreshold { MinElo = 2700, Tier = RankTier.Diamant,  Division = RankDivision.Two   },
        new RankThreshold { MinElo = 2850, Tier = RankTier.Diamant,  Division = RankDivision.One   },
        new RankThreshold { MinElo = 3000, Tier = RankTier.Maitre,   Division = RankDivision.Three },
        new RankThreshold { MinElo = 3200, Tier = RankTier.Maitre,   Division = RankDivision.Two   },
        new RankThreshold { MinElo = 3400, Tier = RankTier.Maitre,   Division = RankDivision.One   },
        new RankThreshold { MinElo = 3600, Tier = RankTier.Champion, Division = RankDivision.None  },
    ];

    /// <summary>
    /// Question pools per rank tier.
    /// Defines which difficulty IDs and optional theme restriction apply during a ranked match.
    /// The pool used is the one matching the <b>highest</b> tier among the two matched players.
    /// </summary>
    public List<RankPool> Pools { get; set; } =
    [
        // Fer     — General theme only, Facile only
        new RankPool { Tier = RankTier.Fer,      DifficultyIds = [1],       ThemeIds = [1] },
        // Bronze  — General theme only, Facile only
        new RankPool { Tier = RankTier.Bronze,   DifficultyIds = [1],       ThemeIds = [1] },
        // Argent  — All themes, Facile + Moyen
        new RankPool { Tier = RankTier.Argent,   DifficultyIds = [1, 2],    ThemeIds = null },
        // Or      — All themes, Facile + Moyen
        new RankPool { Tier = RankTier.Or,       DifficultyIds = [1, 2],    ThemeIds = null },
        // Platine — All themes, Facile + Moyen + Difficile
        new RankPool { Tier = RankTier.Platine,  DifficultyIds = [1, 2, 3], ThemeIds = null },
        // Diamant — All themes, Moyen + Difficile
        new RankPool { Tier = RankTier.Diamant,  DifficultyIds = [2, 3],    ThemeIds = null },
        // Maître  — All themes, Moyen + Difficile + Extrême
        new RankPool { Tier = RankTier.Maitre,   DifficultyIds = [2, 3, 4], ThemeIds = null },
        // Champion — All themes, Difficile + Extrême
        new RankPool { Tier = RankTier.Champion, DifficultyIds = [3, 4],    ThemeIds = null },
    ];

    /// <summary>
    /// Returns the question pool for the given tier.
    /// Falls back to the first configured pool if the tier is not explicitly listed.
    /// </summary>
    public RankPool GetPool(RankTier p_Tier)
        => Pools.FirstOrDefault(p_P => p_P.Tier == p_Tier)
           ?? Pools.FirstOrDefault()
           ?? new RankPool { DifficultyIds = [], ThemeIds = null };

    public string GetSectionName() => SectionName;
}

/// <summary>
/// Defines the minimum Elo required to reach a specific tier and division.
/// </summary>
public class RankThreshold
{
    /// <summary>Minimum Elo (inclusive) to be in this rank.</summary>
    public int MinElo { get; set; }

    /// <summary>Rank tier (Fer, Bronze, Argent, …).</summary>
    public RankTier Tier { get; set; }

    /// <summary>Division within the tier (Three = lowest, One = highest, None for Champion).</summary>
    public RankDivision Division { get; set; }
}

/// <summary>
/// Defines the question pool (difficulty filter + optional theme filter) used during
/// a ranked match at a given tier.
/// </summary>
public class RankPool
{
    /// <summary>Rank tier this pool applies to.</summary>
    public RankTier Tier { get; set; }

    /// <summary>
    /// Difficulty IDs allowed for questions in this tier.
    /// Maps to <c>Difficulty.Id</c> in the database (1=Facile, 2=Moyen, 3=Difficile, 4=Extrême).
    /// </summary>
    public int[] DifficultyIds { get; set; } = [];

    /// <summary>
    /// Optional theme restriction. When <c>null</c> or empty, all themes are eligible.
    /// When set, only questions associated with one of these theme IDs are considered.
    /// </summary>
    public int[]? ThemeIds { get; set; }
}
