namespace TikTakToe.Models;

/// <summary>
/// Represents a durable job for processing an engine move in a game.
/// </summary>
public sealed class EngineMoveJobModel
{
    /// <summary>
    /// Gets or sets the primary identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the target game identifier.
    /// </summary>
    public Guid GameId { get; set; }

    /// <summary>
    /// Gets or sets the job status.
    /// </summary>
    public string Status { get; set; } = JobStatus.Pending;

    /// <summary>
    /// Gets or sets the number of processing attempts.
    /// </summary>
    public int AttemptCount { get; set; } = 0;

    /// <summary>
    /// Gets or sets the maximum allowed attempts before marking as failed.
    /// </summary>
    public int MaxAttempts { get; set; } = 5;

    /// <summary>
    /// Gets or sets when the job was created in UTC.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the job is available for processing in UTC.
    /// Used for backoff retries.
    /// </summary>
    public DateTime AvailableAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when processing started in UTC.
    /// </summary>
    public DateTime? StartedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets when the job completed in UTC.
    /// </summary>
    public DateTime? CompletedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the last error message (truncated).
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Gets or sets the lease owner identifier (machine/process id for debugging).
    /// </summary>
    public string? LeaseOwner { get; set; }

    /// <summary>
    /// Gets or sets when the lease expires in UTC.
    /// Used for stale processing recovery.
    /// </summary>
    public DateTime? LeaseExpiresAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the navigation to the game.
    /// </summary>
    public GameModel? Game { get; set; }
}

/// <summary>
/// Job status constants.
/// </summary>
public static class JobStatus
{
    /// <summary>
    /// Job is waiting to be processed.
    /// </summary>
    public const string Pending = "Pending";

    /// <summary>
    /// Job is currently being processed.
    /// </summary>
    public const string Processing = "Processing";

    /// <summary>
    /// Job completed successfully.
    /// </summary>
    public const string Completed = "Completed";

    /// <summary>
    /// Job failed after max attempts.
    /// </summary>
    public const string Failed = "Failed";

    /// <summary>
    /// Job was cancelled (e.g., game already advanced).
    /// </summary>
    public const string Cancelled = "Cancelled";
}
