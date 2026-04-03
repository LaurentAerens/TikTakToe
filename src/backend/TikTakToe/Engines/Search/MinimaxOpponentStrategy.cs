namespace TikTakToe.Engines.Search;

/// <summary>
/// Both players play perfectly. This is the standard minimax assumption.
/// </summary>
public sealed class MinimaxOpponentStrategy : IOpponentStrategy
{
    public int AggregateScores(IReadOnlyList<int> scores, int currentPlayer, int enginePlayer)
        => currentPlayer == 1 ? scores.Max() : scores.Min();
}
