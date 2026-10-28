namespace TikTakToe.Engines;

using TikTakToe.Engines.Evaluation;
using TikTakToe.Engines.Search;

/// <summary>
/// Halfsight engine using heuristic (half-depth) evaluation.
/// The engine plays Opportunity-style and uses a mild depth discount so near-term wins matter slightly more.
/// </summary>
public sealed class HalfsightEngine : MinimaxEngineBase
{
    public HalfsightEngine()
        : base(new HalfDepthBoardEvaluator(), new DepthDiscountOpponentStrategy())
    {
    }
}
