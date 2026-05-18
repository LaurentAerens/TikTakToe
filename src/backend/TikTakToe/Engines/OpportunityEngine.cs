namespace TikTakToe.Engines;

using TikTakToe.Engines.Evaluation;
using TikTakToe.Engines.Search;

/// <summary>
/// Opportunity engine using classical (terminal-only) evaluation.
/// The engine plays optimally; the opponent is modelled as random (averaged outcomes).
/// </summary>
public sealed class OpportunityEngine : MinimaxEngineBase
{
    public OpportunityEngine()
        : base(new ClassicalBoardEvaluator(), new OpportunityOpponentStrategy())
    {
    }
}
