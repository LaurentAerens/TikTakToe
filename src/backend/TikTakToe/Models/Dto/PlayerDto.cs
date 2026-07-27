namespace TikTakToe.Models.Dto;

/// <summary>
/// Data transfer object representing a player.
/// </summary>
/// <param name="Id">The unique identifier of the player.</param>
/// <param name="IsEngine">Indicates whether the player is an engine.</param>
/// <param name="ExternalId">The external identifier of the engine if applicable.</param>
public sealed record PlayerDto(Guid Id, bool IsEngine, string? ExternalId);
