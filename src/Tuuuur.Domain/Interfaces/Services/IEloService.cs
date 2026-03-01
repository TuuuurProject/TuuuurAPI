namespace Tuuuur.Domain.Interfaces.Services;

public interface IEloService
{
    /// <summary>
    /// Returns the K-factor for the given Elo rating based on the configured thresholds.
    /// </summary>
    int GetKFactor(int p_Elo);

    /// <summary>
    /// Computes the Elo deltas for a win/loss result.
    /// Both values are returned as positive magnitudes.
    /// The caller is responsible for adding WinnerDelta to the winner and subtracting LoserDelta from the loser.
    /// </summary>
    (int WinnerDelta, int LoserDelta) ComputeEloDelta(int p_WinnerElo, int p_LoserElo);
}
