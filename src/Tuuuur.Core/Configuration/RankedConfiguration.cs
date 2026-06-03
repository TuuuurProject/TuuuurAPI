using Tuuuur.Domain.Configuration;

namespace Tuuuur.Core.Configuration;

/// <summary>
/// Configuration for the ranked game mode damage multiplier system.
/// From round 1 to <see cref="ThresholdRound"/>, the multiplier stays at 1×.
/// After that threshold, each additional round increases the damage multiplier
/// by <see cref="MultiplierIncrement"/>, making penalties progressively heavier.
/// </summary>
public class RankedConfiguration : IServiceConfiguration
{
    private const string SectionName = "RankedConfiguration";

    /// <summary>
    /// The round (1-based, inclusive) up to which the damage multiplier stays at 1.
    /// From the very next round onward the multiplier starts increasing.
    /// Default: 5.
    /// </summary>
    public int ThresholdRound { get; set; } = 5;

    /// <summary>
    /// How much the damage multiplier grows per round beyond <see cref="ThresholdRound"/>.
    /// Default: 0.5 (i.e. +50 % penalty per extra round).
    /// </summary>
    public double MultiplierIncrement { get; set; } = 0.5;

    /// <summary>
    /// Initial score assigned to each player in the Redis sorted set at the start of a ranked party.
    /// Default: 5000.
    /// </summary>
    public int InitialRankedScore { get; set; } = 5000;

    /// <summary>
    /// Default Elo value used when a player has no Elo entry for a given theme,
    /// and as the starting Elo assigned to every theme when a new user registers.
    /// Default: 1000.
    /// </summary>
    public int DefaultElo { get; set; }

    public string GetSectionName() => SectionName;
}
