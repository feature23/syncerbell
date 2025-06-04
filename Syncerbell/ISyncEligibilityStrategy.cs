namespace Syncerbell;

/// <summary>
/// A strategy that determines if a sync operation is eligible based on the given trigger.
/// <para />
/// This strategy should return whether the entity is eligible for synchronization at all, not whether any
/// particular data needs to be synchronized. For example, an entity may be eligible for sync with an interval
/// of 1 day if more than 1 day has passed, but if no data has changed since the last sync, no data
/// will be synchronized.
/// <para />
/// Implementations SHOULD NOT query the sync log or any other data store to determine if the entity
/// is eligible for synchronization. They should only consider the provided <see cref="SyncTrigger"/>
/// and perhaps any external state that is relevant to the eligibility of the entity for sync.
/// </summary>
public interface ISyncEligibilityStrategy
{
    /// <summary>
    /// Determines if the entity is eligible for synchronization based on the provided trigger.
    /// <para />
    /// Note that this method is not called for manual sync triggers, as they are always eligible.
    /// </summary>
    /// <param name="trigger">The sync trigger.</param>
    /// <param name="entityOptions">The configured options for the entity to be synchronized.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>Returns a <see cref="ValueTask{TResult}"/> that resolves to <c>true</c> if the entity is eligible for synchronization, otherwise <c>false</c>.</returns>
    ValueTask<bool> IsEligibleToSync(SyncTrigger trigger,
        SyncEntityOptions entityOptions,
        CancellationToken cancellationToken = default);
}
