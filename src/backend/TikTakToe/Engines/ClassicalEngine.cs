namespace TikTakToe.Engines;

using TikTakToe.Engines.Evaluation;

/// <summary>
/// Classical minimax engine for 3x3 Tic-Tac-Toe.
/// Defaults to full-resolution search (searches remaining empty squares) when no depth is provided.
/// The root expansion is evaluated in parallel to utilize multiple cores; deeper recursion is sequential.
/// When a search depth limit is hit during recursion, nodes return score 0 (non-terminal heuristic).
/// </summary>
public sealed class ClassicalEngine : MinimaxEngineBase
{
    public ClassicalEngine()
        : base(new ClassicalBoardEvaluator())
    {
    }
}
