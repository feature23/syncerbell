namespace Syncerbell;

/// <summary>
/// A strategy that determines if a sync operation is eligible based on a specified time interval.
/// <para />
/// The interval is considered the time that must elapse since the last sync operation was leased for the entity
/// and its parameters (if specified).
/// </summary>
/// <param name="interval">The time interval that must elapse since the last sync for the entity to be eligible for synchronization.</param>
public class IntervalEligibilityStrategy(TimeSpan interval) : ISyncEligibilityStrategy
{
    /// <summary>
    /// Gets the time interval that must elapse since the last sync for the entity to be eligible for synchronization.
    /// </summary>
    public TimeSpan Interval { get; } = interval;

    /// <summary>
    /// Determines whether the entity is eligible for synchronization based on the configured interval.
    /// </summary>
    /// <param name="trigger">The trigger that initiated the sync check, including prior sync info.</param>
    /// <param name="entityOptions">The options for the entity being checked.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="ValueTask{Boolean}"/> indicating whether the entity is eligible for synchronization.</returns>
    public ValueTask<bool> IsEligibleToSync(SyncTrigger trigger,
        SyncEntityOptions entityOptions,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromResult(false);
        }

        if (trigger.PriorSyncInfo.LastSyncLeasedAt is null)
        {
            return ValueTask.FromResult(true);
        }

        var elapsed = DateTime.UtcNow - trigger.PriorSyncInfo.LastSyncLeasedAt.Value;

        return ValueTask.FromResult(elapsed >= Interval);
    }
}
