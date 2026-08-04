namespace TikTakToe.Tournament;

/// <summary>
/// Configuration for running a tournament.
/// </summary>
public class TournamentConfig
{
    public List<string> EngineIds { get; set; } = [];

    public int GamesPerPair { get; set; } = 10;

    public int? SearchDepth { get; set; } = null;

    public double InitialElo { get; set; } = 1000.0;

    /// <summary>
    /// Gets or sets step size used when fitting final ELO to the full result set (after all games finish).
    /// </summary>
    public double KFactor { get; set; } = EloCalculator.DefaultKFactor;

    /// <summary>
    /// Gets or sets number of batch passes used when fitting final ELO to the full result set.
    /// </summary>
    public int EloPasses { get; set; } = EloCalculator.DefaultPasses;
}
