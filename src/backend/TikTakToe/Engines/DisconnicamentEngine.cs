using TikTakToe.Engines.Evaluation;
using TikTakToe.Engines.Search;

namespace TikTakToe.Engines;

/// <summary>
/// Disconnicament engine using half-depth evaluation.
/// Intentionally plays to lose while modelling opponent turns as averaged outcomes.
/// </summary>
public sealed class DisconnicamentEngine : MaxminEngineBase
{
    public DisconnicamentEngine() : base(new HalfDepthBoardEvaluator(), new MaxminOpportunityOpponentStrategy())
    {
    }
}
