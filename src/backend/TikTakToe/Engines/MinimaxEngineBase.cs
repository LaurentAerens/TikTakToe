using System.Threading.Tasks;
using TikTakToe.Engines.Evaluation;
using TikTakToe.Engines.Interface;
using TikTakToe.Engines.Exceptions;
using TikTakToe.Engines.Search;

namespace TikTakToe.Engines;

/// <summary>
/// Abstract base class for minimax-based engines.
/// Provides the complete minimax search algorithm with parallel root expansion.
/// Concrete engines compose this base with an <see cref="IBoardEvaluator"/> implementation.
/// </summary>
public abstract class MinimaxEngineBase : IEngine
{
    private readonly IBoardEvaluator _boardEvaluator;
    private readonly IOpponentStrategy _opponentStrategy;

    protected MinimaxEngineBase(IBoardEvaluator boardEvaluator, IOpponentStrategy? opponentStrategy = null)
    {
        _boardEvaluator = boardEvaluator;
        _opponentStrategy = opponentStrategy ?? new MinimaxOpponentStrategy();
    }

    public (int[,] Board, int Score) Move(int[,] board, int player, int? depth = null)
    {
        // This engine only supports 3x3 boards. Reject other sizes early.
        if (board.GetLength(0) != 3 || board.GetLength(1) != 3)
        {
            throw new BoardSizeNotSupportedException(nameof(MinimaxEngineBase), board.GetLength(0), board.GetLength(1));
        }

        // Count empty squares so we can default to a full-resolution search when depth is not provided.
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
            // No legal moves.
            throw new NoMoveAvailableException();
        }

        // Default depth: full resolution (remaining plies) unless caller provided an override.
        var useDepth = depth ?? remaining;
        return StartMinimax(board, player, useDepth);
    }

    public int Eval(int[,] board, int player, int? depth = null)
    {
        // Ensure this engine only evaluates 3x3 boards
        if (board.GetLength(0) != 3 || board.GetLength(1) != 3)
        {
            throw new BoardSizeNotSupportedException(nameof(MinimaxEngineBase), board.GetLength(0), board.GetLength(1));
        }
        if (depth.HasValue)
        {
            var (_, score) = MinimaxRecursive(board, player, depth.Value, player);
            return score;
        }

        return _boardEvaluator.Evaluate(board);
    }

    /// <summary>
    /// Top-level Minimax entrypoint. Evaluates the immediate child moves in parallel
    /// (to leverage multiple cores) and chooses the best move. Deeper recursion uses
    /// <see cref="MinimaxRecursive"/> which is sequential.
    /// </summary>
    private (int[,] Board, int Score) StartMinimax(int[,] board, int player, int depth)
    {
        var enginePlayer = player;
        var moves = GenerateMoves(board, player);

        var bestMove = moves[0];
        var bestScore = player == 1 ? int.MinValue : int.MaxValue;

        // Parallel evaluation at root: evaluate each child's subtree in parallel,
        // then pick the best result. This strikes a balance between CPU utilization
        // and task overhead by limiting parallelism to the top level.
        if (moves.Count > 1)
        {
            var scores = new int[moves.Count];

            Parallel.For(0, moves.Count, i =>
            {
                var (_, moveScore) = MinimaxRecursive(moves[i], ChangePlayer(player), depth - 1, enginePlayer);
                scores[i] = moveScore;
            });

            for (var i = 0; i < moves.Count; i++)
            {
                var moveScore = scores[i];
                if (player == 1)
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

        // Single-move sequential evaluation: directly return that move with its score.
        {
            var (_, moveScore) = MinimaxRecursive(moves[0], ChangePlayer(player), depth - 1, enginePlayer);
            return (moves[0], moveScore);
        }
    }

    /// <summary>
    /// Recursive Minimax used for non-root nodes. Stops when a terminal win/loss
    /// is detected (returns +/-1000). If the search depth reaches zero before a
    /// terminal node, returns 0 (neutral heuristic as per design choice).
    /// The opponent aggregation strategy controls how non-engine nodes score their children.
    /// </summary>
    private (int[,] Board, int Score) MinimaxRecursive(int[,] board, int player, int depth, int enginePlayer)
    {
        var score = _boardEvaluator.Evaluate(board);
        if (score == 1000 || score == -1000)
        {
            return (board, score);
        }

        if (depth == 0)
        {
            // Depth limit reached: return neutral score (non-terminal).
            return (board, 0);
        }

        var moves = GenerateMoves(board, player);
        if (moves.Count == 0)
        {
            // No moves left -> draw/neutral.
            return (board, 0);
        }

        // Collect all child scores, then let the strategy decide how to aggregate.
        // The returned board is always discarded by callers, so moves[0] is a safe placeholder.
        var childScores = new int[moves.Count];
        for (var i = 0; i < moves.Count; i++)
        {
            var (_, moveScore) = MinimaxRecursive(moves[i], ChangePlayer(player), depth - 1, enginePlayer);
            childScores[i] = moveScore;
        }

        var aggregated = _opponentStrategy.AggregateScores(childScores, player, enginePlayer);
        return (moves[0], aggregated);
    }

    private List<int[,]> GenerateMoves(int[,] board, int player)
    {
        // Return a list of boards that result from placing `player` in each empty cell.
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

    private int ChangePlayer(int player)
    {
        return player == 1 ? 2 : 1;
    }

}
