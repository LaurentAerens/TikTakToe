namespace TikTakToe.Engines;

using TikTakToe.Engines.Evaluation;
using TikTakToe.Engines.Search;

/// <summary>
/// Blindsight engine using classical (terminal-only) evaluation.
/// The engine intentionally plays the worst move while still using the mild depth discount model.
/// </summary>
public sealed class BlindsightEngine : MaxminEngineBase
{
    public BlindsightEngine()
        : base(new ClassicalBoardEvaluator(), new DepthDiscountOpponentStrategy())
    {
    }
}
