namespace TikTakToe.Models.Dto;

/// <summary>
/// Request payload to create a new player for testing.
/// </summary>
/// <param name="IsEngine">Indicates whether the player is an AI/engine.</param>
/// <param name="ExternalId">The external identifier of the engine if applicable.</param>
public sealed record CreatePlayerRequest(bool IsEngine, string? ExternalId = null);
