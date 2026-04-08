namespace TikTakToe.Models;

/// <summary>
/// Represents a move made in a game.
/// </summary>
public sealed class MoveModel
{
    /// <summary>
    /// Gets or sets the primary identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the related game identifier.
    /// </summary>
    public Guid GameId { get; set; }

    /// <summary>
    /// Gets or sets the X coordinate.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Gets or sets the Y coordinate.
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// Gets or sets the move value.
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// Gets or sets the move order within the game.
    /// </summary>
    public int MoveNumber { get; set; }

    /// <summary>
    /// Gets or sets when the move was created in UTC.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the related game.
    /// </summary>
    public GameModel Game { get; set; } = null!;
}