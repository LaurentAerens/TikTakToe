using Npgsql;
using NpgsqlTypes;

namespace TikTakToe.Data;

/// <summary>
/// Persists game boards via direct Npgsql commands to support rectangular arrays.
/// </summary>
public sealed class NpgsqlGameBoardStore(NpgsqlDataSource dataSource) : IGameBoardStore
{
    /// <inheritdoc />
    public async Task SetBoardAsync(Guid gameId, int[,] board, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE games
            SET board = @board,
                updated_at_utc = NOW() AT TIME ZONE 'UTC'
            WHERE id = @id;
            """;

        await using var cmd = dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("id", gameId);
        cmd.Parameters.Add(new NpgsqlParameter("board", NpgsqlDbType.Array | NpgsqlDbType.Integer)
        {
            Value = board,
        });

        var affectedRows = await cmd.ExecuteNonQueryAsync(cancellationToken);
        if (affectedRows == 0)
        {
            throw new KeyNotFoundException($"Game '{gameId}' was not found.");
        }
    }

    /// <inheritdoc />
    public async Task<int[,]?> GetBoardAsync(Guid gameId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT board FROM games WHERE id = @id;";

        await using var cmd = dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("id", gameId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken) || reader.IsDBNull(0))
        {
            return null;
        }

        return reader.GetFieldValue<int[,]>(0);
    }
}