namespace TikTakToe.Models.Dto;

/// <summary>
/// Request payload to make a human move.
/// Engine moves are automatic and handled by the background worker.
/// </summary>
/// <param name="X">The row coordinate of the move.</param>
/// <param name="Y">The column coordinate of the move.</param>
public sealed record MakeMoveRequest(int X, int Y);
