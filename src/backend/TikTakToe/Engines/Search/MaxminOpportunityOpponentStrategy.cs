namespace TikTakToe.Engines.Search;

/// <summary>
/// Maxmin opportunity strategy: the engine intentionally chooses the worst result for itself,
/// while opponent turns are averaged to model non-perfect play.
/// </summary>
public sealed class MaxminOpportunityOpponentStrategy : IOpponentStrategy
{
    public int AggregateScores(IReadOnlyList<int> scores, int currentPlayer, int enginePlayer)
    {
        if (currentPlayer == enginePlayer)
        {
            return enginePlayer == 1 ? scores.Min() : scores.Max();
        }

        return (int)Math.Round(scores.Average(s => (double)s));
    }
}
