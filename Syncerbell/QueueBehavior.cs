namespace Syncerbell;

/// <summary>
/// Specifies the behavior for determining which entities should be queued for sync processing.
/// </summary>
public enum QueueBehavior
{
    /// <summary>
    /// Queue all entities regardless of their eligibility status.
    /// This will create sync entries for every registered entity without checking sync eligibility strategies.
    /// </summary>
    QueueAll,

    /// <summary>
    /// Queue only entities that are eligible for sync according to their configured eligibility strategies.
    /// This respects sync eligibility rules such as interval-based strategies or custom eligibility logic.
    /// </summary>
    QueueEligibleOnly

}
