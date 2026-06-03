using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Configuration;
using Tuuuur.Domain.Interfaces.Services;

namespace Tuuuur.Infrastructure.Services;

/// <summary>
/// Resolves a player's Tier and Division from their GlobalElo,
/// and provides the matching question pool for ranked game sessions.
/// </summary>
public class RankService(RankConfiguration p_RankConfiguration) : IRankService
{
    /// <inheritdoc/>
    public (int Tier, int Division) GetRankForElo(int p_Elo)
        => p_RankConfiguration.GetRankForElo(p_Elo);

    /// <inheritdoc/>
    public RankPoolConfiguration GetPoolForTier(int p_Tier)
        => p_RankConfiguration.GetPoolForTier(p_Tier);

    /// <inheritdoc/>
    public int GetAverageTier(int p_Elo1, int p_Elo2)
    {
        (int v_Tier1, _) = GetRankForElo(p_Elo1);
        (int v_Tier2, _) = GetRankForElo(p_Elo2);

        // Average the two tiers, rounded to nearest integer, clamped to [1, max tier]
        int v_MaxTier = p_RankConfiguration.Pools.Count > 0
            ? p_RankConfiguration.Pools.Max(p_P => p_P.Tier)
            : 8;

        int v_Avg = (int)Math.Round((v_Tier1 + v_Tier2) / 2.0);
        return Math.Clamp(v_Avg, 1, v_MaxTier);
    }
}
