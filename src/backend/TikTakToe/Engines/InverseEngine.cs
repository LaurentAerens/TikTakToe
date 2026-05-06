namespace TikTakToe.Engines;

using TikTakToe.Engines.Evaluation;

/// <summary>
/// Inverse engine for 3x3 Tic-Tac-Toe.
/// Intentionally plays to lose using terminal-only evaluation.
/// </summary>
public sealed class InverseEngine : MaxminEngineBase
{
    public InverseEngine()
        : base(new ClassicalBoardEvaluator())
    {
    }
}
