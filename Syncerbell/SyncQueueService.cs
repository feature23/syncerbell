using Microsoft.Extensions.Logging;

namespace Syncerbell;

/// <summary>
/// Implementation of ISyncQueueService that creates queued sync log entries for distributed processing.
/// </summary>
public class SyncQueueService(
    SyncEntityResolver entityResolver,
    ISyncLogPersistence syncLogPersistence,
    ILogger<SyncQueueService> logger)
    : ISyncQueueService
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<ISyncLogEntry>> CreateAllQueuedSyncEntries(SyncTriggerType syncTrigger,
        CancellationToken cancellationToken = default)
    {
        var entities = await entityResolver.ResolveEntities(cancellationToken);

        if (entities.Count == 0)
        {
            logger.LogWarning("No entities registered for sync. Skipping queue creation operation.");
            return [];
        }

        var queuedEntries = new List<ISyncLogEntry>();
        var currentTime = DateTime.UtcNow;

        foreach (var entity in entities)
        {
            try
            {
                var acquireResult = await syncLogPersistence.TryAcquireLogEntry(syncTrigger, entity, AcquireLeaseBehavior.DoNotAcquire, cancellationToken);

                if (acquireResult is not { SyncLogEntry: { } logEntry })
                {
                    logger.LogDebug("No log entry acquired for entity {EntityName}. Skipping queue creation.", entity.Entity);
                    continue;
                }

                // Mark the entry as queued
                logEntry.SyncStatus = SyncStatus.Pending;
                logEntry.QueuedAt = currentTime;

                await syncLogPersistence.UpdateLogEntry(logEntry, cancellationToken);

                queuedEntries.Add(logEntry);

                logger.LogDebug("Created queued sync entry for entity {EntityName}.", entity.Entity);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create queued sync entry for entity {EntityName}.", entity.Entity);
                // Continue with other entities even if one fails
            }
        }

        logger.LogInformation("Created {Count} queued sync entries for distributed processing.", queuedEntries.Count);

        return queuedEntries;
    }

    /// <inheritdoc />
    public async Task RecordQueueMessageId(string syncLogEntryId, string queueMessageId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(syncLogEntryId);
        ArgumentException.ThrowIfNullOrWhiteSpace(queueMessageId);

        var syncLogEntry = await syncLogPersistence.FindById(syncLogEntryId, cancellationToken);

        if (syncLogEntry == null)
        {
            logger.LogWarning("Sync log entry with identifier {Id} not found.", syncLogEntryId);
            throw new InvalidOperationException($"Sync log entry with identifier '{syncLogEntryId}' not found.");
        }

        syncLogEntry.QueueMessageId = queueMessageId;

        await syncLogPersistence.UpdateLogEntry(syncLogEntry, cancellationToken);

        logger.LogDebug("Recorded queue message ID {QueueMessageId} for sync log entry with identifier {Id}.",
            queueMessageId, syncLogEntryId);
    }
}
