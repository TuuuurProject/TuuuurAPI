using Tuuuur.Domain.Bo;

namespace Tuuuur.Domain.Interfaces.Services;

/// <summary>
/// Service that resolves a player's Tier and Division from their ELO,
/// and provides the question pool associated with a given Tier.
/// </summary>
public interface IRankService
{
    /// <summary>
    /// Computes the Tier and Division for a given ELO value.
    /// </summary>
    /// <param name="p_Elo">Player's GlobalElo.</param>
    /// <returns>
    /// A tuple (Tier, Division).
    /// Division is 0 for Tier 8 (top rank — no subdivision).
    /// </returns>
    (int Tier, int Division) GetRankForElo(int p_Elo);

    /// <summary>
    /// Returns the question pool (DifficultyIds + ThemeIds) for a given Tier.
    /// </summary>
    /// <param name="p_Tier">Tier number (1–8).</param>
    RankPoolConfiguration GetPoolForTier(int p_Tier);

    /// <summary>
    /// Computes the average Tier between two players' ELOs,
    /// used to determine which question pool to apply when the two players differ in rank.
    /// The average is rounded to the nearest integer and clamped to [1, 8].
    /// </summary>
    /// <param name="p_Elo1">First player's GlobalElo.</param>
    /// <param name="p_Elo2">Second player's GlobalElo.</param>
    /// <returns>The Tier to use for question selection.</returns>
    int GetAverageTier(int p_Elo1, int p_Elo2);
}
