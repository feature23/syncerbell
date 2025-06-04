namespace Syncerbell;

/// <summary>
/// Represents a service for synchronizing entities.
/// <para />
/// Implementations of this interface should perform whatever synchronization logic is necessary for the entity
/// they are responsible for. The provided <see cref="SyncTrigger"/> will provide context for the sync operation,
/// such as the last sync time and any parameters that were provided when the sync was triggered. Running a sync
/// should be considered an idempotent operation, meaning that running the sync multiple times with the same trigger
/// will not produce different results or duplicate data.
/// </summary>
public interface IEntitySync
{
    /// <summary>
    /// Runs the synchronization operation for the entity.
    /// </summary>
    /// <param name="trigger">The trigger that initiated the sync operation.</param>
    /// <param name="entityOptions">The configured options for the entity to be synchronized.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>Returns a <see cref="Task{TResult}"/> that resolves to a <see cref="SyncResult"/> indicating the outcome of the sync operation. The result must not be null.</returns>
    Task<SyncResult> Run(SyncTrigger trigger, SyncEntityOptions entityOptions, CancellationToken cancellationToken = default);
}
