namespace Syncerbell;

/// <summary>
/// Represents the status of a synchronization operation.
/// </summary>
public enum SyncStatus
{
    /// <summary>
    /// The sync operation is pending and has not started yet.
    /// </summary>
    Pending = 1,
    /// <summary>
    /// The sync operation is currently in progress.
    /// </summary>
    InProgress = 2,
    /// <summary>
    /// The sync operation completed successfully.
    /// </summary>
    Completed = 3,
    /// <summary>
    /// The sync operation failed.
    /// </summary>
    Failed = 4,
    /// <summary>
    /// The lease for the sync operation has expired.
    /// </summary>
    LeaseExpired = 5,
    /// <summary>
    /// The sync operation was skipped (e.g., not eligible).
    /// </summary>
    Skipped = 6,
}
