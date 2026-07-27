namespace TikTakToe.Engines;

using TikTakToe.Engines.Evaluation;
using TikTakToe.Engines.Search;

/// <summary>
/// Sightline engine using classical (terminal-only) evaluation.
/// The engine plays Opportunity-style and uses a mild depth discount so near-term wins matter slightly more.
/// </summary>
public sealed class SightlineEngine : MinimaxEngineBase
{
    public SightlineEngine()
        : base(new ClassicalBoardEvaluator(), new DepthDiscountOpponentStrategy())
    {
    }
}
