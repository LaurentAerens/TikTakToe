namespace TikTakToe.Engines.Search;

/// <summary>
/// Oppertunity strategy: the engine plays optimally, while opponent turns are averaged.
/// This maximises the chance the opponent makes a mistake rather than assuming they play perfectly.
/// </summary>
public sealed class OppertunityOpponentStrategy : IOpponentStrategy
{
    public int AggregateScores(IReadOnlyList<int> scores, int currentPlayer, int enginePlayer)
    {
        if (currentPlayer == enginePlayer)
        {
            // Engine's own turn: still pick the best outcome.
            return enginePlayer == 1 ? scores.Max() : scores.Min();
        }

        // Opponent's turn: take the average, assuming they won't necessarily play optimally.
        return (int)Math.Round(scores.Average(s => (double)s));
    }
}
