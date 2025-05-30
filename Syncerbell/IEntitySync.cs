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
    Task<SyncResult> Run(SyncTrigger trigger, SyncEntityOptions entityOptions, CancellationToken cancellationToken = default);
}
