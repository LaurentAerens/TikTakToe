namespace TikTakToe.Models.Dto;

public sealed record EvalRequest(Guid EngineId, int[][]? Board, int Player, int? Depth = null);
