namespace TikTakToe.Engines;

using TikTakToe.Engines.Evaluation;
using TikTakToe.Engines.Search;

/// <summary>
/// Abstract base class for maxmin-based engines.
/// Maxmin engines intentionally play the move that is worst for themselves.
/// </summary>
public abstract class MaxminEngineBase : SearchEngineBase
{
    protected MaxminEngineBase(IBoardEvaluator boardEvaluator, IOpponentStrategy? opponentStrategy = null)
        : base(boardEvaluator, opponentStrategy ?? new MaxminOpponentStrategy())
    {
    }

    /// <summary>
    /// Maxmin inverts the normal logic: minimizes for player 1, maximizes for player 2
    /// to intentionally play the worst move.
    /// </summary>
    /// <returns></returns>
    protected override bool ShouldMaximize(int player, int enginePlayer)
    {
        return enginePlayer == 2;
    }
}
