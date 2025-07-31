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
    /// <remarks>
    /// Implementations should not modify the trigger type on an existing sync log entry if it differs from the one
    /// provided. This value is only used when creating a new log entry.
    /// </remarks>
    /// <param name="triggerType">The type of trigger that initiated the sync operation.</param>
    /// <param name="entity">The entity for which to acquire a log entry.</param>
    /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>Asynchronously returns the acquired log entry details, or null.</returns>
    Task<AcquireLogEntryResult?> TryAcquireLogEntry(SyncTriggerType triggerType,
        SyncEntityOptions entity,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tries to acquire a provided log entry.
    /// If the log entry exists and is not already leased, it will be returned and leased to the caller.
    /// If the log entry is already leased or does not exist, this method will return null.
    /// </summary>
    /// <param name="syncLogEntry">The sync log entry to acquire.</param>
    /// <param name="entity">The entity options for the log entry to acquire.</param>
    /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>Asynchronously returns the acquired log entry details, or null.</returns>
    Task<AcquireLogEntryResult?> TryAcquireLogEntry(ISyncLogEntry syncLogEntry,
        SyncEntityOptions entity,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the specified log entry in the persistence layer.
    /// </summary>
    /// <param name="logEntry">The log entry to update.</param>
    /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
    Task UpdateLogEntry(ISyncLogEntry logEntry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a sync log entry by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier used to locate the sync log entry.</param>
    /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The sync log entry if found, otherwise null.</returns>
    Task<ISyncLogEntry?> FindById(string id, CancellationToken cancellationToken = default);
}
