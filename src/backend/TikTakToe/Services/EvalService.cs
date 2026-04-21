using TikTakToe.Engines.Exceptions;

namespace TikTakToe.Services;

/// <summary>
/// Default implementation for board evaluation orchestration.
/// </summary>
public sealed class EvalService(IEngineLookupProvider engineLookupProvider) : IEvalService
{
    private const int EmptyCellValue = 0;

    /// <inheritdoc />
    public async Task<int> EvaluateAsync(Guid engineId, int[][]? board, int player, int? depth = null, CancellationToken cancellationToken = default)
    {
        var capability = await engineLookupProvider.GetByIdAsync(engineId, cancellationToken);
        if (capability is null)
        {
            throw new KeyNotFoundException("Engine id not found.");
        }

        var engine = await engineLookupProvider.CreateEngineByIdAsync(engineId, cancellationToken);
        if (engine is null)
        {
            throw new KeyNotFoundException("Engine id not found.");
        }

        var supportedPlayers = NormalizeSupportedPlayers(engine.SupportedPlayers);
        ValidatePlayer(player, supportedPlayers);
        ValidateBoard(board, supportedPlayers);
        var validatedBoard = board!;

        if (validatedBoard.Length > capability.MaxBoardSizeX || validatedBoard[0].Length > capability.MaxBoardSizeY)
        {
            throw new ArgumentException(
                $"Board dimensions exceed engine limits. Max rows={capability.MaxBoardSizeX}, max cols={capability.MaxBoardSizeY}.",
                nameof(board));
        }

        var effectiveDepth = NormalizeDepth(depth);

        var twoDimensionalBoard = ToMultiDimensional(validatedBoard);

        try
        {
            return engine.Eval(twoDimensionalBoard, player, effectiveDepth);
        }
        catch (BoardSizeNotSupportedException ex)
        {
            throw new ArgumentException(ex.Message, nameof(board), ex);
        }
        catch (UnsupportedDepthException ex)
        {
            throw new ArgumentException(ex.Message, nameof(depth), ex);
        }
    }

    private static int? NormalizeDepth(int? depth)
    {
        if (!depth.HasValue || depth.Value == 0)
        {
            return null;
        }

        if (depth.Value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(depth), "Depth must be greater than or equal to 0.");
        }

        return depth.Value;
    }

    private static HashSet<int> NormalizeSupportedPlayers(IReadOnlyCollection<int> supportedPlayers)
    {
        var normalized = supportedPlayers
            .Where(x => x > EmptyCellValue)
            .ToHashSet();

        if (normalized.Count == 0)
        {
            throw new InvalidOperationException("Engine did not declare any supported player values.");
        }

        return normalized;
    }

    private static void ValidatePlayer(int player, IReadOnlySet<int> supportedPlayers)
    {
        if (supportedPlayers.Contains(player))
        {
            return;
        }

        throw new ArgumentOutOfRangeException(nameof(player), $"Player '{player}' is not supported by this engine. Supported players: {string.Join(", ", supportedPlayers.Order())}.");
    }

    private static void ValidateBoard(int[][]? board, IReadOnlySet<int> supportedPlayers)
    {
        if (board is null)
        {
            throw new ArgumentException("Board is required.", nameof(board));
        }

        if (board.Length == 0)
        {
            throw new ArgumentException("Board must have at least one row.", nameof(board));
        }

        if (board[0] is null || board[0].Length == 0)
        {
            throw new ArgumentException("Board must have at least one column.", nameof(board));
        }

        var cols = board[0].Length;
        for (var row = 0; row < board.Length; row++)
        {
            var currentRow = board[row];
            if (currentRow is null)
            {
                throw new ArgumentException($"Board row {row} is missing.", nameof(board));
            }

            if (currentRow.Length != cols)
            {
                throw new ArgumentException("Board rows must have equal length.", nameof(board));
            }

            for (var col = 0; col < cols; col++)
            {
                var value = currentRow[col];
                if (value == EmptyCellValue)
                {
                    continue;
                }

                if (!supportedPlayers.Contains(value))
                {
                    throw new ArgumentException($"Board contains unsupported player value '{value}'. Supported players: {string.Join(", ", supportedPlayers.Order())}.", nameof(board));
                }
            }
        }
    }

    private static int[,] ToMultiDimensional(int[][] board)
    {
        var rows = board.Length;
        var cols = board[0].Length;
        var result = new int[rows, cols];

        for (var row = 0; row < rows; row++)
        {
            for (var col = 0; col < cols; col++)
            {
                result[row, col] = board[row][col];
            }
        }

        return result;
    }
}
