using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Bo.Enum;
using Tuuuur.Domain.Interfaces.Services;

namespace Tuuuur.Infrastructure.Services;

/// <summary>
/// Standard Elo rating service with a configurable, variable K-factor.
/// Formula:
///   E = 1 / (1 + 10^((opponentElo - playerElo) / 400))
///   delta = K * (S - E)   where S = 1 (win) or 0 (loss)
/// </summary>
public class EloService(EloConfiguration p_EloConfiguration, RankConfiguration p_RankConfiguration) : IEloService
{
    /// <inheritdoc/>
    public int GetKFactor(int p_Elo) => GetKFactor(p_Elo, int.MaxValue);

    /// <inheritdoc/>
    public int GetKFactor(int p_Elo, int p_GamesPlayed)
    {
        if (p_GamesPlayed < p_EloConfiguration.PlacementGames)
            return p_EloConfiguration.PlacementKFactor;

        foreach (KFactorThreshold v_Threshold in p_EloConfiguration.KFactorThresholds.OrderBy(p_T => p_T.MaxElo))
        {
            if (p_Elo <= v_Threshold.MaxElo)
                return v_Threshold.K;
        }

        return p_EloConfiguration.KFactorThresholds.OrderByDescending(p_T => p_T.MaxElo).First().K;
    }

    /// <inheritdoc/>
    public (int WinnerDelta, int LoserDelta) ComputeEloDelta(int p_WinnerElo, int p_LoserElo)
        => ComputeEloDelta(p_WinnerElo, p_LoserElo, int.MaxValue, int.MaxValue);

    /// <inheritdoc/>
    public (int WinnerDelta, int LoserDelta) ComputeEloDelta(int p_WinnerElo, int p_LoserElo, int p_WinnerGamesPlayed, int p_LoserGamesPlayed)
    {
        // Expected score for winner (probability of winning against this opponent)
        double v_ExpectedWinner = 1.0 / (1.0 + Math.Pow(10, (p_LoserElo - p_WinnerElo) / 400.0));
        // Expected score for loser
        double v_ExpectedLoser = 1.0 - v_ExpectedWinner;

        int v_KWinner = GetKFactor(p_WinnerElo, p_WinnerGamesPlayed);
        int v_KLoser = GetKFactor(p_LoserElo, p_LoserGamesPlayed);

        // S = 1 for winner, S = 0 for loser
        int v_WinnerDelta = (int)Math.Round(v_KWinner * (1.0 - v_ExpectedWinner));
        int v_LoserDelta = (int)Math.Round(v_KLoser * v_ExpectedLoser); // magnitude only (positive)

        return (v_WinnerDelta, v_LoserDelta);
    }

    /// <inheritdoc/>
    public Rank GetRank(int p_Elo)
    {
        RankThreshold v_Match = p_RankConfiguration.Thresholds
            .OrderByDescending(p_T => p_T.MinElo)
            .FirstOrDefault(p_T => p_Elo >= p_T.MinElo)
            ?? p_RankConfiguration.Thresholds.OrderBy(p_T => p_T.MinElo).First();

        return new Rank { Tier = v_Match.Tier, Division = v_Match.Division };
    }

    /// <inheritdoc/>
    public RankedQuestionPool GetHighestPool(int p_Elo1, int p_Elo2)
    {
        Rank v_Rank1 = GetRank(p_Elo1);
        Rank v_Rank2 = GetRank(p_Elo2);
        RankTier v_HigherTier = (RankTier)Math.Max((int)v_Rank1.Tier, (int)v_Rank2.Tier);
        RankPool v_Pool = p_RankConfiguration.GetPool(v_HigherTier);
        return new RankedQuestionPool(v_Pool.DifficultyIds, v_Pool.ThemeIds);
    }
}
