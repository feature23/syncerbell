namespace Syncerbell;

/// <summary>
/// Service for creating sync log entries that are marked as queued for distributed, asynchronous processing.
/// This service enables fanning out sync operations across multiple workers or services using a FIFO queue pattern.
/// </summary>
public interface ISyncQueueService
{
    /// <summary>
    /// Creates sync log entries for all entities and marks them as queued for processing.
    /// The entries are created in a queued state, ready to be picked up by distributed workers
    /// for asynchronous sync processing in FIFO order.
    /// </summary>
    /// <remarks>
    /// This does not actually enqueue the entries in a message queue or similar system.
    /// The caller should do that separately, and then call RecordQueueMessage to record the queue message
    /// details in the sync log.
    /// </remarks>
    /// <param name="syncTrigger">The type of trigger that initiated this sync operation (e.g., manual, timer).</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation if needed.</param>
    /// <returns>
    /// A read-only list of sync log entries that were created and queued for processing.
    /// Each entry represents a sync operation that needs to be performed by a distributed worker.
    /// </returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token.</exception>
    Task<IReadOnlyList<ISyncLogEntry>> CreateAllQueuedSyncEntries(SyncTriggerType syncTrigger,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records queue message details in the sync log entry after it has been enqueued in a message queue system.
    /// This method should be called after successfully enqueuing a sync log entry to track the queue message information.
    /// </summary>
    /// <param name="syncLogEntryId">The unique identifier used to locate the sync log entry in the system, such as a sync log ID or entity name.</param>
    /// <param name="queueMessageId">The unique identifier of the message in the queue system.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation if needed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when syncLogEntry or queueMessageId is null.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token.</exception>
    Task RecordQueueMessageId(string syncLogEntryId, string queueMessageId,
        CancellationToken cancellationToken = default);
}
