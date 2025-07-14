namespace Syncerbell;

/// <summary>
/// Represents the result of acquiring a log entry for synchronization.
/// </summary>
/// <param name="SyncLogEntry">The synchronization log entry for which the result is being returned.</param>
/// <param name="PriorSyncInfo">Information about the prior synchronization state, including high water mark and timestamps.</param>
public record AcquireLogEntryResult(ISyncLogEntry SyncLogEntry, PriorSyncInfo PriorSyncInfo);
