namespace TikTakToe.Engines.Search;

/// <summary>
/// Maxmin strategy: the engine intentionally chooses the outcome that is worst for itself,
/// while non-engine turns still assume each player acts in their own interest.
/// </summary>
public sealed class MaxminOpponentStrategy : IOpponentStrategy
{
    public int AggregateScores(IReadOnlyList<int> scores, int currentPlayer, int enginePlayer)
    {
        if (currentPlayer == enginePlayer)
        {
            return enginePlayer == 1 ? scores.Min() : scores.Max();
        }

        return currentPlayer == 1 ? scores.Max() : scores.Min();
    }
}
