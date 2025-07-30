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

    /// <summary>
    /// Synchronizes a specific entity if it's eligible for syncing based on the provided trigger type.
    /// </summary>
    /// <param name="triggerType">The type of trigger that initiated the sync operation.</param>
    /// <param name="entity">The entity options for the entity to sync.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>Returns a task that resolves to a <see cref="SyncResult"/> object indicating the outcome of the sync operation, or null if the entity was not eligible or could not be processed.</returns>
    Task<SyncResult?> SyncEntityIfEligible(SyncTriggerType triggerType,
        SyncEntityOptions entity,
        CancellationToken cancellationToken = default);
}
