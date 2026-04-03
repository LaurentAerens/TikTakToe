using TikTakToe.Engines.Evaluation;

namespace TikTakToe.Engines;

/// <summary>
/// Minimax engine using a half-depth heuristic evaluator for non-terminal positions.
/// </summary>
public sealed class HalfDepthEngine : MinimaxEngineBase
{
    public HalfDepthEngine() : base(new HalfDepthBoardEvaluator())
    {
    }
}
