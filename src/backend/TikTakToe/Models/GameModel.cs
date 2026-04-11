namespace TikTakToe.Models;

/// <summary>
/// Represents a persisted game.
/// </summary>
public sealed class GameModel
{
    /// <summary>
    /// Gets or sets the primary identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets when the game was created in UTC.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the game was last updated in UTC.
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the game players.
    /// </summary>
    public List<PlayerModel> Players { get; set; } = [];

    /// <summary>
    /// Gets or sets the game moves.
    /// </summary>
    public List<MoveModel> Moves { get; set; } = [];

    /// <summary>
    /// Gets or sets the in-memory board as a rectangular array.
    /// This property is persisted by EF Core to PostgreSQL jsonb.
    /// </summary>
    public int[,]? Board { get; set; }
}