using Microsoft.Extensions.Logging;

namespace Syncerbell;

public class SyncService(
    SyncerbellOptions options,
    EntitySyncResolver entitySyncResolver,
    ISyncLogPersistence syncLogPersistence,
    ILogger<SyncService> logger)
    : ISyncService
{
    public async Task SyncAllIfEligible(SyncTriggerType triggerType, CancellationToken cancellationToken = default)
    {
        var entities = options.Entities;

        if (entities.Count == 0)
        {
            logger.LogWarning("No entities registered for sync. Skipping sync operation.");
            return;
        }

        foreach (var entity in entities)
        {
            await SyncEntityIfEligible(triggerType, entity, cancellationToken);
        }
    }

    private async Task SyncEntityIfEligible(SyncTriggerType triggerType,
        SyncEntityOptions entity,
        CancellationToken cancellationToken = default)
    {
        var acquireResult = await syncLogPersistence.TryAcquireLogEntry(entity, cancellationToken);

        if (acquireResult is not { SyncLogEntry: { } log, PriorSyncInfo: { } priorSyncInfo })
        {
            // If no log entry was acquired, we skip the sync for this entity.
            // This could be because the entity is already being processed or has a pending sync.
            logger.LogDebug("No log entry acquired for entity {EntityName}. Skipping sync.", entity.Entity);
            return;
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

            await UpdateLogEntry(entity, log, SyncStatus.Skipped, "Entity is not eligible for sync", cancellationToken);

            return;
        }

        logger.LogInformation(
            "Entity {EntityName} is eligible for sync. Trigger type: {TriggerType}.",
            entity.Entity, triggerType);

        var sync = entitySyncResolver.Resolve(entity);

        try
        {
            var syncResult = await sync.Run(trigger, entity, cancellationToken);

            if (!syncResult.Success)
            {
                logger.LogWarning("Sync for entity {EntityName} failed with message: {Message}",
                    entity.Entity, syncResult.Message);

                await UpdateLogEntry(entity, log, SyncStatus.Failed, syncResult.Message ?? "The sync was unsuccessful", cancellationToken);

                return;
            }

            logger.LogInformation("Sync for entity {EntityName} completed successfully.", entity.Entity);

            await UpdateLogEntry(entity, log, SyncStatus.Completed, syncResult.Message ?? "The sync was successful", cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during sync for entity {EntityName} with trigger type {TriggerType}.",
                entity.Entity, triggerType);

            await UpdateLogEntry(entity, log, SyncStatus.Failed, ex.Message, cancellationToken);
        }
    }

    private async Task UpdateLogEntry(SyncEntityOptions entity, ISyncLogEntry log, SyncStatus status, string resultMessage, CancellationToken cancellationToken)
    {
        log.SyncStatus = status;
        log.ResultMessage = resultMessage;
        log.FinishedAt = DateTime.UtcNow;
        await syncLogPersistence.UpdateLogEntry(entity, log, cancellationToken);
    }
}
