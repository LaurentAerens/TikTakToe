using System.Threading.Tasks;
using TikTakToe.Engines.Interface;
using TikTakToe.Engines.Exceptions;

namespace TikTakToe.Engines;

/// <summary>
/// Classical minimax engine for 3x3 Tic-Tac-Toe.
/// Defaults to full-resolution search (searches remaining empty squares) when no depth is provided.
/// The root expansion is evaluated in parallel to utilize multiple cores; deeper recursion is sequential.
/// When a search depth limit is hit during recursion, nodes return score 0 (non-terminal heuristic).
/// </summary>
public sealed class ClassicalEngine : IEngine
{
    public (int[,] Board, int Score) Move(int[,] board, int player, int? depth = null)
    {
        // This engine only supports 3x3 boards. Reject other sizes early.
        if (board.GetLength(0) != 3 || board.GetLength(1) != 3)
        {
            throw new BoardSizeNotSupportedException(nameof(ClassicalEngine), board.GetLength(0), board.GetLength(1));
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
            throw new BoardSizeNotSupportedException(nameof(ClassicalEngine), board.GetLength(0), board.GetLength(1));
        }
        if (depth.HasValue)
        {
            var (_, score) = MinimaxRecursive(board, player, depth.Value);
            return score;
        }

        return EvalBoard(board);
    }

    /// <summary>
    /// Top-level Minimax entrypoint. Evaluates the immediate child moves in parallel
    /// (to leverage multiple cores) and chooses the best move. Deeper recursion uses
    /// <see cref="MinimaxRecursive"/> which is sequential.
    /// </summary>
    private (int[,] Board, int Score) StartMinimax(int[,] board, int player, int depth)
    {
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
                var (_, moveScore) = MinimaxRecursive(moves[i], ChangePlayer(player), depth - 1);
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
            var (_, moveScore) = MinimaxRecursive(moves[0], ChangePlayer(player), depth - 1);
            return (moves[0], moveScore);
        }
    }

    /// <summary>
    /// Recursive Minimax used for non-root nodes. Stops when a terminal win/loss
    /// is detected (returns +/-1000). If the search depth reaches zero before a
    /// terminal node, returns 0 (neutral heuristic as per design choice).
    /// </summary>
    private (int[,] Board, int Score) MinimaxRecursive(int[,] board, int player, int depth)
    {
        var score = EvalBoard(board);
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

        var bestMove = moves[0];
        var bestScore = player == 1 ? int.MinValue : int.MaxValue;

        foreach (var move in moves)
        {
            var (_, moveScore) = MinimaxRecursive(move, ChangePlayer(player), depth - 1);
            if (player == 1)
            {
                if (moveScore > bestScore)
                {
                    bestScore = moveScore;
                    bestMove = move;
                }
            }
            else
            {
                if (moveScore < bestScore)
                {
                    bestScore = moveScore;
                    bestMove = move;
                }
            }
        }

        return (bestMove, bestScore);
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

    private int EvalBoard(int[,] board)
    {
        // Fast center-first checks: many winning lines include the center cell.
        // Checking center ownership first is a small optimization.
        if (board[1, 1] == 1 &&
            (
                (board[0, 0] == 1 && board[2, 2] == 1) ||
                (board[0, 2] == 1 && board[2, 0] == 1) ||
                (board[0, 1] == 1 && board[2, 1] == 1) ||
                (board[1, 0] == 1 && board[1, 2] == 1)
            ))
        {
            return 1000;
        }

        if (board[1, 1] == 2 &&
            (
                (board[0, 0] == 2 && board[2, 2] == 2) ||
                (board[0, 2] == 2 && board[2, 0] == 2) ||
                (board[0, 1] == 2 && board[2, 1] == 2) ||
                (board[1, 0] == 2 && board[1, 2] == 2)
            ))
        {
            return -1000;
        }

        // Row and column wins (non-center dependent)
        if ((board[0,0] == 1 && board[0,1] == 1 && board[0,2] == 1) ||
            (board[1,0] == 1 && board[1,1] == 1 && board[1,2] == 1) ||
            (board[2,0] == 1 && board[2,1] == 1 && board[2,2] == 1) ||
            (board[0,0] == 1 && board[1,0] == 1 && board[2,0] == 1) ||
            (board[0,1] == 1 && board[1,1] == 1 && board[2,1] == 1) ||
            (board[0,2] == 1 && board[1,2] == 1 && board[2,2] == 1))
        {
            return 1000;
        }

        if ((board[0,0] == 2 && board[0,1] == 2 && board[0,2] == 2) ||
            (board[1,0] == 2 && board[1,1] == 2 && board[1,2] == 2) ||
            (board[2,0] == 2 && board[2,1] == 2 && board[2,2] == 2) ||
            (board[0,0] == 2 && board[1,0] == 2 && board[2,0] == 2) ||
            (board[0,1] == 2 && board[1,1] == 2 && board[2,1] == 2) ||
            (board[0,2] == 2 && board[1,2] == 2 && board[2,2] == 2))
        {
            return -1000;
        }
        return 0;
    }
      
}