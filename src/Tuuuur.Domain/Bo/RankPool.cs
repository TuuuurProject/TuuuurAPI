namespace Tuuuur.Domain.Bo;

/// <summary>
/// Represents one question pool tied to a specific Tier.
/// Defines which difficulties (and optionally which themes) are used for ranked questions
/// at that Tier level.
/// </summary>
public class RankPoolConfiguration
{
    /// <summary>Tier this pool applies to (1–8).</summary>
    public int Tier { get; set; }

    /// <summary>
    /// IDs of the difficulties allowed for questions in this pool.
    /// Maps to the <c>Difficulty_DIF.Id</c> column in the database.
    /// </summary>
    public List<int> DifficultyIds { get; set; } = [];

    /// <summary>
    /// Optional list of theme IDs to restrict questions to.
    /// When <c>null</c>, all themes are eligible.
    /// Maps to the <c>Theme_THM.Id</c> column in the database.
    /// </summary>
    public List<int> ThemeIds { get; set; }
}

/// <summary>
/// Maps a minimum ELO value to a Tier and Division.
/// Thresholds must be ordered by <see cref="MinElo"/> ascending in configuration.
/// </summary>
public class RankThresholdConfiguration
{
    /// <summary>Minimum ELO (inclusive) to be placed at this Tier/Division.</summary>
    public int MinElo { get; set; }

    /// <summary>Tier number (1–8).</summary>
    public int Tier { get; set; }

    /// <summary>
    /// Division within the Tier (1–3, descending prestige: 1 = best).
    /// Division 0 is used exclusively for Tier 8 (no subdivision — top rank).
    /// </summary>
    public int Division { get; set; }
}
