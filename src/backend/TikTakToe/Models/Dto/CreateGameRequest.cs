namespace TikTakToe.Models.Dto;

/// <summary>
/// Request payload to create a new game.
/// </summary>
/// <param name="Rows">The number of rows on the board.</param>
/// <param name="Cols">The number of columns on the board.</param>
/// <param name="PlayerIds">The IDs of the players participating in the game.</param>
public sealed record CreateGameRequest(int Rows = 3, int Cols = 3, Guid[]? PlayerIds = null);
