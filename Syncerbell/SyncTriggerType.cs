namespace Syncerbell;

/// <summary>
/// Specifies the type of trigger that initiates a synchronization operation.
/// </summary>
public enum SyncTriggerType
{
    /// <summary>
    /// An unknown trigger type, used when the trigger is not specified or recognized.
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// A timer-based trigger, such as a scheduled or periodic sync.
    /// </summary>
    Timer = 1,
    /// <summary>
    /// A manual trigger, such as a user-initiated sync operation.
    /// </summary>
    Manual = 2,
}
