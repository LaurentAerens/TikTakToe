namespace TikTakToe.Tournament;

/// <summary>
/// Represents the performance statistics for a single engine in the tournament.
/// </summary>
public class EngineStatistics
{
    public string EngineId { get; set; } = string.Empty;

    public string EngineName { get; set; } = string.Empty;

    public int GamesPlayed { get; set; }

    public int Wins { get; set; }

    public int Losses { get; set; }

    public int Draws { get; set; }

    public double WinRate => this.GamesPlayed > 0 ? (double)this.Wins / this.GamesPlayed : 0;

    public double InitialElo { get; set; }

    public double CurrentElo { get; set; }

    public double EloChange => this.CurrentElo - this.InitialElo;

    public int TotalMoves { get; set; }

    public double AverageMovesPerGame => this.GamesPlayed > 0 ? (double)this.TotalMoves / this.GamesPlayed : 0;
}
