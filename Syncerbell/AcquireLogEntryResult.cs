namespace Syncerbell;

public record AcquireLogEntryResult(ISyncLogEntry SyncLogEntry, PriorSyncInfo PriorSyncInfo);
