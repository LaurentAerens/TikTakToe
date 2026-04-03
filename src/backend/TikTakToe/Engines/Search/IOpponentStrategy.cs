namespace TikTakToe.Engines.Search;

/// <summary>
/// Controls how the opponent's node scores are aggregated during minimax recursion.
/// Implement this to change what assumption the engine makes about how the opponent plays.
/// </summary>
public interface IOpponentStrategy
{
    /// <summary>
    /// Aggregates child move scores for the current search node.
    /// </summary>
    /// <param name="scores">Scores of all legal moves at this node.</param>
    /// <param name="currentPlayer">The player moving at this node (1 or 2).</param>
    /// <param name="enginePlayer">The player the engine is playing as (constant throughout the search).</param>
    int AggregateScores(IReadOnlyList<int> scores, int currentPlayer, int enginePlayer);
}
