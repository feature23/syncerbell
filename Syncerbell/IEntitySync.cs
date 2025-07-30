namespace Syncerbell;

/// <summary>
/// Represents a service for synchronizing entities.
/// <para />
/// Implementations of this interface should perform whatever synchronization logic is necessary for the entity
/// they are responsible for. The provided <see cref="EntitySyncContext"/> will provide context for the sync operation,
/// including the trigger information and entity options. Running a sync should be considered an idempotent operation,
/// meaning that running the sync multiple times with the same context will not produce different results or duplicate data.
/// </summary>
public interface IEntitySync
{
    /// <summary>
    /// Runs the synchronization operation for the entity.
    /// </summary>
    /// <param name="context">The context for the sync operation.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>Returns a <see cref="Task{TResult}"/> that resolves to a <see cref="SyncResult"/> indicating the outcome of the sync operation. The result must not be null.</returns>
    Task<SyncResult> Run(EntitySyncContext context, CancellationToken cancellationToken = default);
}
