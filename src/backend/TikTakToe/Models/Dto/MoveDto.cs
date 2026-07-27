namespace TikTakToe.Models.Dto;

/// <summary>
/// Data transfer object representing a move.
/// </summary>
/// <param name="Id">The unique identifier of the move.</param>
/// <param name="X">The row coordinate.</param>
/// <param name="Y">The column coordinate.</param>
/// <param name="Value">The player value assigned to this cell.</param>
/// <param name="MoveNumber">The sequential move number.</param>
public sealed record MoveDto(Guid Id, int X, int Y, int Value, int MoveNumber);
