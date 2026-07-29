namespace TikTakToe.Models.Dto;

/// <summary>
/// Data transfer object describing terminal state information for a game.
/// </summary>
/// <param name="IsGameOver">Whether the game has reached a terminal state (win or draw).</param>
/// <param name="WinnerValue">Winner marker: null when game is in progress, -1 when game ended in a draw, or positive board value (for example 1 or 2) when a player won.</param>
/// <param name="WinnerPlayerId">The player ID of the winner, or null when there is no winner (draw or in-progress).</param>
public sealed record GameStateDto(
    bool IsGameOver,
    int? WinnerValue,
    Guid? WinnerPlayerId);
