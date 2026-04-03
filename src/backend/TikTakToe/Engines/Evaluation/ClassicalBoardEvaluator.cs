namespace TikTakToe.Engines.Evaluation;

public sealed class ClassicalBoardEvaluator : IBoardEvaluator
{
    public int Evaluate(int[,] board)
    {
        return BoardEvaluationPrimitives.EvaluateTerminalState(board);
    }
}