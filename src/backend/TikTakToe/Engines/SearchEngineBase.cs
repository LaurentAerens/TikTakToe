using TikTakToe.Engines.Evaluation;
using TikTakToe.Engines.Exceptions;
using TikTakToe.Engines.Interface;
using TikTakToe.Engines.Search;

namespace TikTakToe.Engines;

/// <summary>
/// Abstract base class providing common search logic for engines using tree-based evaluation.
/// Concrete engines specify the opponent strategy and search direction (maximize/minimize).
/// </summary>
public abstract class SearchEngineBase : IEngine
{
    private readonly IBoardEvaluator _boardEvaluator;
    private readonly IOpponentStrategy _opponentStrategy;

    protected SearchEngineBase(IBoardEvaluator boardEvaluator, IOpponentStrategy opponentStrategy)
    {
        _boardEvaluator = boardEvaluator;
        _opponentStrategy = opponentStrategy;
    }

    public (int[,] Board, int Score) Move(int[,] board, int player, int? depth = null)
    {
        if (board.GetLength(0) != 3 || board.GetLength(1) != 3)
        {
            throw new BoardSizeNotSupportedException(GetType().Name, board.GetLength(0), board.GetLength(1));
        }

        var remaining = CountEmptyCells(board);
        if (remaining == 0)
        {
            throw new NoMoveAvailableException();
        }

        var useDepth = depth ?? remaining;
        return StartSearch(board, player, useDepth);
    }

    public int Eval(int[,] board, int player, int? depth = null)
    {
        if (board.GetLength(0) != 3 || board.GetLength(1) != 3)
        {
            throw new BoardSizeNotSupportedException(GetType().Name, board.GetLength(0), board.GetLength(1));
        }

        var useDepth = depth ?? CountEmptyCells(board);
        var (_, score) = SearchRecursive(board, player, useDepth, player);
        return score;
    }

    /// <summary>
    /// Determines whether the given player should maximize or minimize scores.
    /// </summary>
    protected abstract bool ShouldMaximize(int player, int enginePlayer);

    private (int[,] Board, int Score) StartSearch(int[,] board, int player, int depth)
    {
        var enginePlayer = player;
        var moves = GenerateMoves(board, player);

        var shouldMaximize = ShouldMaximize(player, enginePlayer);
        var bestMove = moves[0];
        var bestScore = shouldMaximize ? int.MinValue : int.MaxValue;

        if (moves.Count > 1)
        {
            var scores = new int[moves.Count];

            Parallel.For(0, moves.Count, i =>
            {
                var (_, moveScore) = SearchRecursive(moves[i], ChangePlayer(player), depth - 1, enginePlayer);
                scores[i] = moveScore;
            });

            for (var i = 0; i < moves.Count; i++)
            {
                var moveScore = scores[i];
                if (shouldMaximize)
                {
                    if (moveScore > bestScore)
                    {
                        bestScore = moveScore;
                        bestMove = moves[i];
                    }
                }
                else
                {
                    if (moveScore < bestScore)
                    {
                        bestScore = moveScore;
                        bestMove = moves[i];
                    }
                }
            }

            return (bestMove, bestScore);
        }

        var (_, singleMoveScore) = SearchRecursive(moves[0], ChangePlayer(player), depth - 1, enginePlayer);
        return (moves[0], singleMoveScore);
    }

    private (int[,] Board, int Score) SearchRecursive(int[,] board, int player, int depth, int enginePlayer)
    {
        var score = _boardEvaluator.Evaluate(board);
        if (score == 1000 || score == -1000)
        {
            return (board, score);
        }

        if (depth == 0)
        {
            return (board, score);
        }

        var moves = GenerateMoves(board, player);
        if (moves.Count == 0)
        {
            return (board, 0);
        }

        var childScores = new int[moves.Count];
        for (var i = 0; i < moves.Count; i++)
        {
            var (_, moveScore) = SearchRecursive(moves[i], ChangePlayer(player), depth - 1, enginePlayer);
            childScores[i] = moveScore;
        }

        var aggregated = _opponentStrategy.AggregateScores(childScores, player, enginePlayer);
        return (moves[0], aggregated);
    }

    private static int CountEmptyCells(int[,] board)
    {
        var count = 0;
        for (var x = 0; x < board.GetLength(0); x++)
        {
            for (var y = 0; y < board.GetLength(1); y++)
            {
                if (board[x, y] == 0)
                {
                    count++;
                }
            }
        }
        return count;
    }

    private static List<int[,]> GenerateMoves(int[,] board, int player)
    {
        var moves = new List<int[,]>();
        var rows = board.GetLength(0);
        var cols = board.GetLength(1);

        for (var x = 0; x < rows; x++)
        {
            for (var y = 0; y < cols; y++)
            {
                if (board[x, y] == 0)
                {
                    var newBoard = (int[,])board.Clone();
                    newBoard[x, y] = player;
                    moves.Add(newBoard);
                }
            }
        }

        return moves;
    }

    private static int ChangePlayer(int player)
    {
        return player == 1 ? 2 : 1;
    }
}
