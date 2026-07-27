namespace TikTakToe.Models.Dto;

/// <summary>
/// Data transfer object representing a game state.
/// </summary>
/// <param name="Id">The unique identifier of the game.</param>
/// <param name="Board">The current board state as a jagged array.</param>
/// <param name="Players">The players in the game.</param>
/// <param name="Moves">The moves made in the game.</param>
/// <param name="WaitingForPlayerId">The ID of the player whose turn it is, or null if the game is over.</param>
public sealed record GameDto(
    Guid Id,
    int[][] Board,
    PlayerDto[] Players,
    MoveDto[] Moves,
    Guid? WaitingForPlayerId);
