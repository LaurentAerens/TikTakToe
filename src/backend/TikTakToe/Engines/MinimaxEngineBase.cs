using TikTakToe.Engines.Evaluation;
using TikTakToe.Engines.Search;

namespace TikTakToe.Engines;

/// <summary>
/// Abstract base class for minimax-based engines.
/// Provides the complete minimax search algorithm with parallel root expansion.
/// Concrete engines compose this base with an <see cref="IBoardEvaluator"/> implementation.
/// </summary>
public abstract class MinimaxEngineBase : SearchEngineBase
{
    protected MinimaxEngineBase(IBoardEvaluator boardEvaluator, IOpponentStrategy? opponentStrategy = null)
        : base(boardEvaluator, opponentStrategy ?? new MinimaxOpponentStrategy())
    {
    }

    /// <summary>
    /// Minimax maximizes for player 1 and minimizes for player 2.
    /// </summary>
    protected override bool ShouldMaximize(int player, int enginePlayer)
    {
        return player == 1;
    }
}
