namespace TikTakToe.Engines.Search;

/// <summary>
/// Depth-discount strategy: terminal scores are gently compressed so nearer outcomes matter a bit more
/// than far-future outcomes, without flattening the search tree.
/// </summary>
public sealed class DepthDiscountOpponentStrategy : IOpponentStrategy
{
    public int AggregateScores(IReadOnlyList<int> scores, int currentPlayer, int enginePlayer)
    {
        if (currentPlayer == enginePlayer)
        {
            if (enginePlayer == 1)
            {
                return scores.Select(score => score > 0 ? ReducePositiveScore(score) : score).Max();
            }

            return scores.Select(score => score < 0 ? ReduceNegativeScore(score) : score).Min();
        }

        return (int)Math.Round(scores.Average(score => (double)score));
    }

    private static int ReducePositiveScore(int score)
    {
        return ApplyConfidenceCurve(score);
    }

    private static int ReduceNegativeScore(int score)
    {
        return -ApplyConfidenceCurve(Math.Abs(score));
    }

    private static int ApplyConfidenceCurve(int score)
    {
        var confidence = 0.7f + (0.3f * (score / 1000f));
        return (int)Math.Round(score * confidence);
    }
}
