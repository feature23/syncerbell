namespace Syncerbell;

/// <summary>
/// Represents a log entry for a synchronization operation, including status, timing, and result information.
/// </summary>
public interface ISyncLogEntry
{
    /// <summary>
    /// Gets or sets the current status of the sync operation.
    /// </summary>
    SyncStatus SyncStatus { get; set; }

    /// <summary>
    /// Gets the date and time when the log entry was created.
    /// </summary>
    DateTime CreatedAt { get; }

    /// <summary>
    /// Gets or sets the date and time when the sync operation was leased.
    /// </summary>
    DateTime? LeasedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the lease for the sync operation expires.
    /// </summary>
    DateTime? LeaseExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the machine or process that leased the sync operation.
    /// </summary>
    string? LeasedBy { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the sync operation finished.
    /// </summary>
    DateTime? FinishedAt { get; set; }

    /// <summary>
    /// Gets or sets the result message of the sync operation, if any.
    /// </summary>
    string? ResultMessage { get; set; }

    /// <summary>
    /// Gets or sets the high-water mark value for incremental sync scenarios.
    /// </summary>
    string? HighWaterMark { get; set; }
}
