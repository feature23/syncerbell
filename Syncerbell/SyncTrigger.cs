namespace Syncerbell;

public class SyncTrigger
{
    public required SyncTriggerType TriggerType { get; init; }

    public required PriorSyncInfo PriorSyncInfo { get; init; }
}
