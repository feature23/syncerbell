namespace Syncerbell;

/// <summary>
/// The core interface for interacting with the Syncerbell service.
/// </summary>
public interface ISyncService
{
    /// <summary>
    /// Synchronizes all entities that are eligible for syncing based on the provided trigger type.
    /// </summary>
    /// <param name="triggerType">The type of trigger that initiated the sync operation.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>Returns a task that resolves to a list of <see cref="SyncResult"/> objects indicating the outcome of each sync operation.</returns>
    Task<IReadOnlyList<SyncResult>> SyncAllEligible(
        SyncTriggerType triggerType,
        CancellationToken cancellationToken = default);
}
