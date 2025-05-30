namespace Syncerbell;

/// <summary>
/// An abstraction for a persistence layer that manages synchronization log entries.
/// </summary>
public interface ISyncLogPersistence
{
    /// <summary>
    /// Acquires a log entry for the specified entity from the persistence layer.
    /// This may involve creating a new log entry if an available one does not exist yet.
    /// If there is an unleased log entry for the entity, it will be returned and leased to the caller.
    /// If there is a log entry that is already leased, this method will return null.
    /// </summary>
    /// <param name="entity">The entity for which to acquire a log entry.</param>
    /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>Asynchronously returns the acquired log entry, or null.</returns>
    Task<AcquireLogEntryResult> TryAcquireLogEntry(SyncEntityOptions entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the specified log entry in the persistence layer.
    /// </summary>
    /// <param name="entity">The options for the entity associated with the log entry.</param>
    /// <param name="logEntry">The log entry to update.</param>
    /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
    Task UpdateLogEntry(SyncEntityOptions entity, ISyncLogEntry logEntry, CancellationToken cancellationToken = default);
}
