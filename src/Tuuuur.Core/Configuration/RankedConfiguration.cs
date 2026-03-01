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

    public string GetSectionName() => SectionName;
}
