namespace Syncerbell;

public interface ISyncLogEntry
{
    SyncStatus SyncStatus { get; set; }

    DateTime CreatedAt { get; }

    DateTime? LeasedAt { get; set; }

    DateTime? LeaseExpiresAt { get; set; }

    string? LeasedBy { get; set; }

    DateTime? FinishedAt { get; set; }

    string? ResultMessage { get; set; }

    string? HighWaterMark { get; set; }
}
