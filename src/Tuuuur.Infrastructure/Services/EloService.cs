using Tuuuur.Domain.Interfaces.Services;

namespace Tuuuur.Infrastructure.Services;

/// <summary>
/// Standard Elo rating service with a configurable, variable K-factor.
/// Formula:
///   E = 1 / (1 + 10^((opponentElo - playerElo) / 400))
///   delta = K * (S - E)   where S = 1 (win) or 0 (loss)
/// </summary>
public class EloService(EloConfiguration p_EloConfiguration) : IEloService
{
    /// <inheritdoc/>
    public int GetKFactor(int p_Elo)
    {
        foreach (KFactorThreshold v_Threshold in p_EloConfiguration.KFactorThresholds.OrderBy(p_T => p_T.MaxElo))
        {
            if (p_Elo <= v_Threshold.MaxElo)
                return v_Threshold.K;
        }

        return p_EloConfiguration.KFactorThresholds.OrderByDescending(p_T => p_T.MaxElo).First().K;
    }

    /// <inheritdoc/>
    public (int WinnerDelta, int LoserDelta) ComputeEloDelta(int p_WinnerElo, int p_LoserElo)
    {
        // Expected score for winner (probability of winning against this opponent)
        double v_ExpectedWinner = 1.0 / (1.0 + Math.Pow(10, (p_LoserElo - p_WinnerElo) / 400.0));
        // Expected score for loser
        double v_ExpectedLoser = 1.0 - v_ExpectedWinner;

        int v_KWinner = GetKFactor(p_WinnerElo);
        int v_KLoser = GetKFactor(p_LoserElo);

        // S = 1 for winner, S = 0 for loser
        int v_WinnerDelta = (int)Math.Round(v_KWinner * (1.0 - v_ExpectedWinner));
        int v_LoserDelta = (int)Math.Round(v_KLoser * v_ExpectedLoser); // magnitude only (positive)

        return (v_WinnerDelta, v_LoserDelta);
    }
}
