namespace TikTakToe.Models;

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
