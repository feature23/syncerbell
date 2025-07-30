using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Syncerbell;

/// <summary>
/// Provides synchronization services for all registered entities, handling eligibility, logging, and execution.
/// </summary>
public class SyncService(
    SyncerbellOptions options,
    IServiceProvider serviceProvider,
    ISyncLogPersistence syncLogPersistence,
    ILogger<SyncService> logger)
    : ISyncService
{
    private const string SyncSuccessMessage = "Sync completed successfully.";
    private const string SyncFailedMessage = "The sync failed. Check logs for details.";

    /// <inheritdoc />
    public async Task<IReadOnlyList<SyncResult>> SyncAllEligible(SyncTriggerType triggerType, CancellationToken cancellationToken = default)
    {
        var entities = new List<SyncEntityOptions>(options.Entities);

        if (options.EntityProviderType is { } entityProviderType)
        {
            var entityProvider = serviceProvider.GetRequiredService(entityProviderType) as IEntityProvider
                ?? throw new InvalidOperationException(
                    $"Entity provider type {entityProviderType.FullName} is not registered or does not implement {nameof(IEntityProvider)}.");

            var additionalEntities = await entityProvider.GetEntities(cancellationToken);

            if (additionalEntities.Count == 0)
            {
                logger.LogWarning("Entity provider returned no additional entities. Using configured entities only.");
            }
            else
            {
                logger.LogInformation("Entity provider returned {Count} additional entities.", additionalEntities.Count);
                entities.AddRange(additionalEntities);
            }
        }

        if (entities.Count == 0)
        {
            logger.LogWarning("No entities registered for sync. Skipping sync operation.");
            return [];
        }

        var results = new List<SyncResult>();

        foreach (var entity in entities)
        {
            var result = await SyncEntityIfEligible(triggerType, entity, cancellationToken);

            if (result is not null)
            {
                results.Add(result);
            }
        }

        logger.LogInformation("Sync operation completed for {Count} entities.", results.Count);

        return results;
    }

    /// <inheritdoc />
    public async Task<SyncResult?> SyncEntityIfEligible(SyncTriggerType triggerType,
        SyncEntityOptions entity,
        CancellationToken cancellationToken = default)
    {
        var acquireResult = await syncLogPersistence.TryAcquireLogEntry(entity, cancellationToken);

        if (acquireResult is not { SyncLogEntry: { } log, PriorSyncInfo: { } priorSyncInfo })
        {
            // If no log entry was acquired, we skip the sync for this entity.
            // This could be because the entity is already being processed or has a pending sync.
            logger.LogDebug("No log entry acquired for entity {EntityName}. Skipping sync.", entity.Entity);
            return null;
        }

        var trigger = new SyncTrigger
        {
            PriorSyncInfo = priorSyncInfo,
            TriggerType = triggerType,
        };

        var isEligibleToSync = triggerType == SyncTriggerType.Manual
                               || await entity.Eligibility.IsEligibleToSync(trigger, entity, cancellationToken);

        if (!isEligibleToSync)
        {
            logger.LogDebug(
                "Entity {EntityName} is not eligible for sync based on the current trigger type {TriggerType}.",
                entity.Entity, triggerType);

            await UpdateLogEntry(log, SyncStatus.Skipped, new SyncResult(entity, Success: false, Message: "Entity is not eligible for sync"), cancellationToken);

            return null;
        }

        logger.LogInformation(
            "Entity {EntityName} is eligible for sync. Trigger type: {TriggerType}.",
            entity.Entity, triggerType);

        var sync = serviceProvider.GetRequiredService(entity.EntitySyncType) as IEntitySync
            ?? throw new InvalidOperationException(
                $"Entity sync type {entity.EntitySyncType.FullName} is not registered or does not implement {nameof(IEntitySync)}.");

        try
        {
            var context = new EntitySyncContext(trigger, entity, (progress) => ReportProgress(entity, log, progress, cancellationToken));
            var syncResult = await sync.Run(context, cancellationToken);

            if (!syncResult.Success)
            {
                logger.LogWarning("Sync for entity {EntityName} failed with message: {Message}",
                    entity.Entity, syncResult.Message);

                await UpdateLogEntry(log, SyncStatus.Failed, syncResult, cancellationToken);

                return syncResult;
            }

            logger.LogInformation("Sync for entity {EntityName} completed successfully.", entity.Entity);

            await UpdateLogEntry(log, SyncStatus.Completed, syncResult, cancellationToken);

            return syncResult;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during sync for entity {EntityName} with trigger type {TriggerType}.",
                entity.Entity, triggerType);

            await UpdateLogEntry(log, SyncStatus.Failed, new SyncResult(entity, Success: false, Message: ex.Message), cancellationToken);

            return new SyncResult(Entity: entity, Success: false, Message: ex.Message);
        }
    }

    /// <summary>
    /// Reports progress for a sync operation by updating the log entry with progress information.
    /// </summary>
    /// <param name="entity">The entity being synchronized.</param>
    /// <param name="logEntry">The log entry to update with progress information.</param>
    /// <param name="progress">The progress information containing current value and maximum.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous progress reporting operation.</returns>
    private async Task ReportProgress(SyncEntityOptions entity, ISyncLogEntry logEntry, Progress progress, CancellationToken cancellationToken)
    {
        logEntry.ProgressValue = progress.Value;
        logEntry.ProgressMax = progress.Max;

        logger.LogDebug("Reporting progress for entity {EntityName}: {ProgressValue}/{ProgressMax} ({ProgressPercentage:P})",
            entity.Entity, progress.Value, progress.Max, logEntry.ProgressPercentage);

        await syncLogPersistence.UpdateLogEntry(entity, logEntry, cancellationToken);
    }

    private async Task UpdateLogEntry(ISyncLogEntry log, SyncStatus status, SyncResult syncResult, CancellationToken cancellationToken)
    {
        log.SyncStatus = status;
        log.ResultMessage = syncResult.Message ?? (syncResult.Success ? SyncSuccessMessage : SyncFailedMessage);
        log.FinishedAt = DateTime.UtcNow;
        log.HighWaterMark = syncResult.HighWaterMark;
        await syncLogPersistence.UpdateLogEntry(syncResult.Entity, log, cancellationToken);
    }
}
