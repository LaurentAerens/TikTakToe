namespace TikTakToe.Engines.Evaluation;

public interface IBoardEvaluator
{
    int Evaluate(int[,] board);
}