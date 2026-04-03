using TikTakToe.Engines.Evaluation;
using TikTakToe.Engines.Search;

namespace TikTakToe.Engines;

/// <summary>
/// Oppertunity engine using classical (terminal-only) evaluation.
/// The engine plays optimally; the opponent is modelled as random (averaged outcomes).
/// </summary>
public sealed class OppertunityEngine : MinimaxEngineBase
{
    public OppertunityEngine() : base(new ClassicalBoardEvaluator(), new OppertunityOpponentStrategy())
    {
    }
}
