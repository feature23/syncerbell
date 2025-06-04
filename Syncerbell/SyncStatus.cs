namespace Syncerbell;

public enum SyncStatus
{
    Pending = 1,
    InProgress = 2,
    Completed = 3,
    Failed = 4,
    LeaseExpired = 5,
    Skipped = 6,
}
