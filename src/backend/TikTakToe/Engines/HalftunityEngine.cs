using TikTakToe.Engines.Evaluation;
using TikTakToe.Engines.Search;

namespace TikTakToe.Engines;

/// <summary>
/// Halftunity engine using heuristic (half-depth) evaluation.
/// The engine plays optimally; the opponent is modelled as random (averaged outcomes).
/// </summary>
public sealed class HalftunityEngine : MinimaxEngineBase
{
    public HalftunityEngine() : base(new HalfDepthBoardEvaluator(), new OpportunityOpponentStrategy())
    {
    }
}
