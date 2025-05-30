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
    public TimeSpan Interval { get; } = interval;

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
