using TikTakToe.Engines.Evaluation;

namespace TikTakToe.Engines;

/// <summary>
/// Disconnected engine for 3x3 Tic-Tac-Toe.
/// Intentionally plays to lose using the half-depth heuristic evaluator.
/// </summary>
public sealed class DisconnectedEngine : MaxminEngineBase
{
    public DisconnectedEngine() : base(new HalfDepthBoardEvaluator())
    {
    }
}
