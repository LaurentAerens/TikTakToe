using Microsoft.EntityFrameworkCore;
using TikTakToe.Models;

namespace TikTakToe.Data;

/// <summary>
/// Entity Framework database context for game persistence.
/// </summary>
public sealed class GameDbContext(DbContextOptions<GameDbContext> options) : DbContext(options)
{
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

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var game = modelBuilder.Entity<GameModel>();
        game.ToTable("games");
        game.HasKey(x => x.Id);
        game.Property(x => x.Id).HasColumnName("id");
        game.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        game.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");
        game.Property<string>("BoardPersisted").HasColumnName("board");

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
        player.Property(x => x.Id).HasColumnName("id").UseIdentityByDefaultColumn();
        player.Property(x => x.GameId).HasColumnName("game_id");
        player.Property(x => x.IsEngine).HasColumnName("is_engine");
        player.Property(x => x.ExternalId).HasColumnName("external_id").HasMaxLength(128);
        player.HasIndex(x => x.GameId);

        var move = modelBuilder.Entity<MoveModel>();
        move.ToTable("moves");
        move.HasKey(x => x.Id);
        move.Property(x => x.Id).HasColumnName("id").UseIdentityByDefaultColumn();
        move.Property(x => x.GameId).HasColumnName("game_id");
        move.Property(x => x.X).HasColumnName("x");
        move.Property(x => x.Y).HasColumnName("y");
        move.Property(x => x.Value).HasColumnName("value");
        move.Property(x => x.MoveNumber).HasColumnName("move_number");
        move.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        move.HasIndex(x => x.GameId);
        move.HasIndex(x => new { x.GameId, x.MoveNumber }).IsUnique();
    }
}