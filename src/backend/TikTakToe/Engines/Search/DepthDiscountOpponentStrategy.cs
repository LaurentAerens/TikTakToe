namespace TikTakToe.Engines.Search;

/// <summary>
/// Depth-discount strategy: terminal scores are gently compressed so nearer outcomes matter a bit more
/// than far-future outcomes, without flattening the search tree.
/// </summary>
public sealed class DepthDiscountOpponentStrategy : IOpponentStrategy
{
    public int AggregateScores(IReadOnlyList<int> scores, int currentPlayer, int enginePlayer)
    {
        var adjustedScores = scores.Select(score =>
        {
            if (enginePlayer == 1 && score < 0)
            {
                return score * 5;
            }

            if (enginePlayer == 2 && score > 0)
            {
                return score * 5;
            }

            return score;
        }).ToList();

        if (currentPlayer == enginePlayer)
        {
            if (enginePlayer == 1)
            {
                return adjustedScores.Select(score => score > 0 ? ReducePositiveScore(score) : score).Max();
            }

            return adjustedScores.Select(score => score < 0 ? ReduceNegativeScore(score) : score).Min();
        }

        var average = adjustedScores.Average(score => (double)score);
        var shouldPenalize = (enginePlayer == 1 && average > 0) || (enginePlayer == 2 && average < 0);
        if (shouldPenalize)
        {
            var best = enginePlayer == 1 ? adjustedScores.Max() : adjustedScores.Min();
            var worst = enginePlayer == 1 ? adjustedScores.Min() : adjustedScores.Max();
            var spread = Math.Abs(best - worst);
            if (spread > 0)
            {
                var penalty = (int)Math.Round(spread * 0.1);
                return enginePlayer == 1 ? (int)Math.Round(average - penalty) : (int)Math.Round(average + penalty);
            }
        }

        return (int)Math.Round(average);
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
