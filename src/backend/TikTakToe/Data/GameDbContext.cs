using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;
using TikTakToe.Models;

namespace TikTakToe.Data;

/// <summary>
/// Entity Framework database context for game persistence.
/// </summary>
public sealed class GameDbContext(DbContextOptions<GameDbContext> options) : DbContext(options)
{
    private static readonly ValueConverter<int[,]?, string?> BoardConverter = new(
        board => SerializeBoard(board),
        payload => DeserializeBoard(payload));

    private static readonly ValueComparer<int[,]?> BoardComparer = new(
        (left, right) => BoardsEqual(left, right),
        board => GetBoardHashCode(board),
        board => CloneBoard(board));

    /// <summary>
    /// Gets or sets games.
    /// </summary>
    public DbSet<GameModel> Games => Set<GameModel>();

    /// <summary>
    /// Gets or sets players.
    /// </summary>
    public DbSet<PlayerModel> Players => Set<PlayerModel>();

    /// <summary>
    /// Gets or sets moves.
    /// </summary>
    public DbSet<MoveModel> Moves => Set<MoveModel>();

    /// <summary>
    /// Gets or sets engine capabilities.
    /// </summary>
    public DbSet<EngineCapabilityModel> EngineCapabilities => Set<EngineCapabilityModel>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var game = modelBuilder.Entity<GameModel>();
        game.ToTable("games");
        game.HasKey(x => x.Id);
        game.Property(x => x.Id).HasColumnName("id");
        game.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        game.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");
        var boardProperty = game.Property(x => x.Board)
            .HasConversion(BoardConverter)
            .Metadata;
        boardProperty.SetValueComparer(BoardComparer);

        game.Property(x => x.Board).HasColumnName("board");
        if (Database.IsNpgsql())
        {
            game.Property(x => x.Board)
                .HasColumnType("jsonb");
        }

        game.HasMany(x => x.Players)
            .WithOne(x => x.Game)
            .HasForeignKey(x => x.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        game.HasMany(x => x.Moves)
            .WithOne(x => x.Game)
            .HasForeignKey(x => x.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        var player = modelBuilder.Entity<PlayerModel>();
        player.ToTable("players");
        player.HasKey(x => x.Id);
        player.Property(x => x.Id).HasColumnName("id");
        player.Property(x => x.GameId).HasColumnName("game_id");
        player.Property(x => x.IsEngine).HasColumnName("is_engine");
        player.Property(x => x.ExternalId).HasColumnName("external_id").HasMaxLength(128);
        player.HasIndex(x => x.GameId);

        var move = modelBuilder.Entity<MoveModel>();
        move.ToTable("moves");
        move.HasKey(x => x.Id);
        move.Property(x => x.Id).HasColumnName("id");
        move.Property(x => x.GameId).HasColumnName("game_id");
        move.Property(x => x.X).HasColumnName("x");
        move.Property(x => x.Y).HasColumnName("y");
        move.Property(x => x.Value).HasColumnName("value");
        move.Property(x => x.MoveNumber).HasColumnName("move_number");
        move.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        move.HasIndex(x => x.GameId);
        move.HasIndex(x => new { x.GameId, x.MoveNumber }).IsUnique();

        var engineCapability = modelBuilder.Entity<EngineCapabilityModel>();
        engineCapability.ToTable("engine_capabilities");
        engineCapability.HasKey(x => x.Id);
        engineCapability.Property(x => x.Id).HasColumnName("id");
        engineCapability.Property(x => x.DisplayName).HasColumnName("display_name").HasMaxLength(128);
        engineCapability.Property(x => x.MaxBoardSizeX).HasColumnName("max_board_size_x");
        engineCapability.Property(x => x.MaxBoardSizeY).HasColumnName("max_board_size_y");
        engineCapability.Property(x => x.Depth).HasColumnName("depth");
        engineCapability.HasIndex(x => x.DisplayName).IsUnique();
    }

    private static string? SerializeBoard(int[,]? board)
    {
        if (board is null)
        {
            return null;
        }

        var rows = board.GetLength(0);
        var cols = board.GetLength(1);
        var serialized = new int[rows][];

        for (var row = 0; row < rows; row++)
        {
            serialized[row] = new int[cols];
            for (var col = 0; col < cols; col++)
            {
                serialized[row][col] = board[row, col];
            }
        }

        return JsonSerializer.Serialize(serialized);
    }

    private static int[,]? DeserializeBoard(string? payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return null;
        }

        var serialized = JsonSerializer.Deserialize<int[][]>(payload)
            ?? throw new InvalidOperationException("Board payload is missing.");

        if (serialized.Length == 0)
        {
            return new int[0, 0];
        }

        var cols = serialized[0].Length;
        for (var row = 1; row < serialized.Length; row++)
        {
            if (serialized[row].Length != cols)
            {
                throw new InvalidOperationException("Board payload must contain rectangular nested arrays.");
            }
        }

        var board = new int[serialized.Length, cols];
        for (var row = 0; row < serialized.Length; row++)
        {
            for (var col = 0; col < cols; col++)
            {
                board[row, col] = serialized[row][col];
            }
        }

        return board;
    }

    private static int[,]? CloneBoard(int[,]? board)
    {
        if (board is null)
        {
            return null;
        }

        return (int[,])board.Clone();
    }

    private static bool BoardsEqual(int[,]? left, int[,]? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        if (left.GetLength(0) != right.GetLength(0) || left.GetLength(1) != right.GetLength(1))
        {
            return false;
        }

        for (var row = 0; row < left.GetLength(0); row++)
        {
            for (var col = 0; col < left.GetLength(1); col++)
            {
                if (left[row, col] != right[row, col])
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static int GetBoardHashCode(int[,]? board)
    {
        if (board is null)
        {
            return 0;
        }

        var hash = new HashCode();
        hash.Add(board.GetLength(0));
        hash.Add(board.GetLength(1));

        for (var row = 0; row < board.GetLength(0); row++)
        {
            for (var col = 0; col < board.GetLength(1); col++)
            {
                hash.Add(board[row, col]);
            }
        }

        return hash.ToHashCode();
    }
}