namespace TikTakToe.Engines.Search;

/// <summary>
/// Opportunity strategy: the engine plays optimally, while opponent turns are averaged.
/// This maximises the chance the opponent makes a mistake rather than assuming they play perfectly.
/// </summary>
public sealed class OpportunityOpponentStrategy : IOpponentStrategy
{
    public int AggregateScores(IReadOnlyList<int> scores, int currentPlayer, int enginePlayer)
    {
        if (currentPlayer == enginePlayer)
        {
            // Engine's own turn: still pick the best outcome.
            return enginePlayer == 1 ? scores.Max() : scores.Min();
        }

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

        // Opponent's turn: take the average, assuming they won't necessarily play optimally.
        return (int)Math.Round(adjustedScores.Average(s => (double)s));
    }
}
