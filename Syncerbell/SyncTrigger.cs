namespace Syncerbell;

/// <summary>
/// Represents a trigger for a synchronization operation, including the trigger type and prior sync information.
/// </summary>
public class SyncTrigger
{
    /// <summary>
    /// Gets the type of trigger that initiated the sync operation.
    /// </summary>
    public required SyncTriggerType TriggerType { get; init; }

    /// <summary>
    /// Gets information about the previous synchronization state for the entity.
    /// </summary>
    public required PriorSyncInfo PriorSyncInfo { get; init; }
}
