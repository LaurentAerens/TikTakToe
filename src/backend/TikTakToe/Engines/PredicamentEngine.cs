using TikTakToe.Engines.Evaluation;
using TikTakToe.Engines.Search;

namespace TikTakToe.Engines;

/// <summary>
/// Predicament engine using classical evaluation.
/// Intentionally plays to lose while modelling opponent turns as averaged outcomes.
/// </summary>
public sealed class PredicamentEngine : MaxminEngineBase
{
    public PredicamentEngine() : base(new ClassicalBoardEvaluator(), new MaxminOpportunityOpponentStrategy())
    {
    }
}
