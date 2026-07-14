namespace TikTakToe.Models.Dto;

/// <summary>
/// Request payload to make a move.
/// </summary>
/// <param name="X">The row coordinate of the move (null for engine/AI moves).</param>
/// <param name="Y">The column coordinate of the move (null for engine/AI moves).</param>
public sealed record MakeMoveRequest(int? X = null, int? Y = null);
