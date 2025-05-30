namespace Syncerbell;

public class PriorSyncInfo
{
    public required string? HighWaterMark { get; init; }

    public required DateTime? LastSyncQueuedAt { get; set; }

    public required DateTime? LastSyncLeasedAt { get; set; }

    public required DateTime? LastSyncCompletedAt { get; set; }
}
