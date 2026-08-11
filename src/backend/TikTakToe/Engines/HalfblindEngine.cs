namespace TikTakToe.Engines;

using TikTakToe.Engines.Evaluation;
using TikTakToe.Engines.Search;

/// <summary>
/// Halfblind engine using heuristic (half-depth) evaluation.
/// The engine intentionally plays the worst move while still using the mild depth discount model.
/// </summary>
public sealed class HalfblindEngine : MaxminEngineBase
{
    public HalfblindEngine()
        : base(new HalfDepthBoardEvaluator(), new DepthDiscountOpponentStrategy())
    {
    }
}
