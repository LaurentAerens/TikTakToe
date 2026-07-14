namespace TikTakToe.Models;

/// <summary>
/// Represents a player participation entry for a game.
/// </summary>
public sealed class GamePlayerModel
{
    /// <summary>
    /// Gets or sets the related game identifier.
    /// </summary>
    public Guid GameId { get; set; }

    /// <summary>
    /// Gets or sets the related player identifier.
    /// </summary>
    public Guid PlayerId { get; set; }

    /// <summary>
    /// Gets or sets the player turn order in the game (0-based).
    /// </summary>
    public int TurnOrder { get; set; }

    /// <summary>
    /// Gets or sets the related game.
    /// </summary>
    public GameModel Game { get; set; } = null!;

    /// <summary>
    /// Gets or sets the related player.
    /// </summary>
    public PlayerModel Player { get; set; } = null!;
}
