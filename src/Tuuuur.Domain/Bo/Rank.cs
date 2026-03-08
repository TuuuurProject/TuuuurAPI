using Tuuuur.Domain.Bo.Enum;

namespace Tuuuur.Domain.Bo;

/// <summary>
/// Represents a player's rank, combining a tier (e.g. Bronze) and a division (e.g. 2).
/// Champion is the only tier with no division (<see cref="RankDivision.None"/>).
/// </summary>
public record Rank
{
    public RankTier Tier { get; init; }

    /// <summary>
    /// Division within the tier (3 = lowest, 1 = highest).
    /// <see cref="RankDivision.None"/> for <see cref="RankTier.Champion"/>.
    /// </summary>
    public RankDivision Division { get; init; }

    /// <summary>Human-readable label, e.g. "Bronze 2" or "Champion".</summary>
    public override string ToString() =>
        Division == RankDivision.None
            ? Tier.ToString()
            : $"{Tier} {(int)Division}";
}
