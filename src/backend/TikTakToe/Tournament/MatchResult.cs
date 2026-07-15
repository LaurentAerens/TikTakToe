namespace TikTakToe.Tournament;

/// <summary>
/// Represents a single match result in the tournament.
/// </summary>
public record MatchResult(
    string Engine1Id,
    string Engine2Id,
    GameResult ResultFromEngine1Perspective,
    int MoveCount);
