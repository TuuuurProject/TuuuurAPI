using Tuuuur.Domain.Bo;

namespace Tuuuur.Domain.Interfaces.Services;

public interface IEloService
{
    /// <summary>
    /// Returns the K-factor for the given Elo rating based on the configured thresholds.
    /// Assumes the player is past the placement phase.
    /// </summary>
    int GetKFactor(int p_Elo);

    /// <summary>
    /// Returns the K-factor for the given Elo rating and number of games played.
    /// Uses the placement K-factor when <paramref name="p_GamesPlayed"/> is below the configured threshold.
    /// </summary>
    int GetKFactor(int p_Elo, int p_GamesPlayed);

    /// <summary>
    /// Computes the Elo deltas for a win/loss result.
    /// Both values are returned as positive magnitudes.
    /// The caller is responsible for adding WinnerDelta to the winner and subtracting LoserDelta from the loser.
    /// </summary>
    (int WinnerDelta, int LoserDelta) ComputeEloDelta(int p_WinnerElo, int p_LoserElo);

    /// <summary>
    /// Computes the Elo deltas taking placement phase into account for each player.
    /// </summary>
    (int WinnerDelta, int LoserDelta) ComputeEloDelta(int p_WinnerElo, int p_LoserElo, int p_WinnerGamesPlayed, int p_LoserGamesPlayed);

    /// <summary>
    /// Returns the <see cref="Rank"/> corresponding to the given Elo value.
    /// </summary>
    Rank GetRank(int p_Elo);

    /// <summary>
    /// Determines the question pool for a ranked match by taking the highest rank
    /// between the two players' Elo values.
    /// The higher-ranked player's pool is used so the match is always challenging.
    /// </summary>
    RankedQuestionPool GetHighestPool(int p_Elo1, int p_Elo2);
}
