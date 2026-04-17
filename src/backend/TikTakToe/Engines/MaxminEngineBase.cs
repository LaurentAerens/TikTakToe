using TikTakToe.Engines.Evaluation;
using TikTakToe.Engines.Exceptions;
using TikTakToe.Engines.Interface;
using TikTakToe.Engines.Search;

namespace TikTakToe.Engines;

/// <summary>
/// Abstract base class for maxmin-based engines.
/// Maxmin engines intentionally play the move that is worst for themselves.
/// </summary>
public abstract class MaxminEngineBase : IEngine
{
    private readonly IBoardEvaluator _boardEvaluator;
    private readonly IOpponentStrategy _opponentStrategy;

    protected MaxminEngineBase(IBoardEvaluator boardEvaluator, IOpponentStrategy? opponentStrategy = null)
    {
        _boardEvaluator = boardEvaluator;
        _opponentStrategy = opponentStrategy ?? new MaxminOpponentStrategy();
    }

    public (int[,] Board, int Score) Move(int[,] board, int player, int? depth = null)
    {
        if (board.GetLength(0) != 3 || board.GetLength(1) != 3)
        {
            throw new BoardSizeNotSupportedException(nameof(MaxminEngineBase), board.GetLength(0), board.GetLength(1));
        }

        var remaining = 0;
        for (var x = 0; x < board.GetLength(0); x++)
        {
            for (var y = 0; y < board.GetLength(1); y++)
            {
                if (board[x, y] == 0)
                {
                    remaining++;
                }
            }
        }

        if (remaining == 0)
        {
            throw new NoMoveAvailableException();
        }

        var useDepth = depth ?? remaining;
        return StartMaxmin(board, player, useDepth);
    }

    public int Eval(int[,] board, int player, int? depth = null)
    {
        if (board.GetLength(0) != 3 || board.GetLength(1) != 3)
        {
            throw new BoardSizeNotSupportedException(nameof(MaxminEngineBase), board.GetLength(0), board.GetLength(1));
        }

        if (depth.HasValue)
        {
            var (_, score) = MaxminRecursive(board, player, depth.Value, player);
            return score;
        }

        return _boardEvaluator.Evaluate(board);
    }

    private (int[,] Board, int Score) StartMaxmin(int[,] board, int player, int depth)
    {
        var enginePlayer = player;
        var moves = GenerateMoves(board, player);

        var bestMove = moves[0];
        var bestScore = enginePlayer == 1 ? int.MaxValue : int.MinValue;

        if (moves.Count > 1)
        {
            var scores = new int[moves.Count];

            Parallel.For(0, moves.Count, i =>
            {
                var (_, moveScore) = MaxminRecursive(moves[i], ChangePlayer(player), depth - 1, enginePlayer);
                scores[i] = moveScore;
            });

            for (var i = 0; i < moves.Count; i++)
            {
                var moveScore = scores[i];
                if (enginePlayer == 1)
                {
                    if (moveScore < bestScore)
                    {
                        bestScore = moveScore;
                        bestMove = moves[i];
                    }
                }
                else
                {
                    if (moveScore > bestScore)
                    {
                        bestScore = moveScore;
                        bestMove = moves[i];
                    }
                }
            }

            return (bestMove, bestScore);
        }

        var (_, singleMoveScore) = MaxminRecursive(moves[0], ChangePlayer(player), depth - 1, enginePlayer);
        return (moves[0], singleMoveScore);
    }

    private (int[,] Board, int Score) MaxminRecursive(int[,] board, int player, int depth, int enginePlayer)
    {
        var score = _boardEvaluator.Evaluate(board);
        if (score == 1000 || score == -1000)
        {
            return (board, score);
        }

        if (depth == 0)
        {
            return (board, 0);
        }

        var moves = GenerateMoves(board, player);
        if (moves.Count == 0)
        {
            return (board, 0);
        }

        var childScores = new int[moves.Count];
        for (var i = 0; i < moves.Count; i++)
        {
            var (_, moveScore) = MaxminRecursive(moves[i], ChangePlayer(player), depth - 1, enginePlayer);
            childScores[i] = moveScore;
        }

        var aggregated = _opponentStrategy.AggregateScores(childScores, player, enginePlayer);
        return (moves[0], aggregated);
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
