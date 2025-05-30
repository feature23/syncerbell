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
        foreach (var entity in options.Entities)
        {
            await SyncEntityIfEligible(triggerType, entity, cancellationToken);
        }
    }

    private async Task SyncEntityIfEligible(SyncTriggerType triggerType,
        SyncEntityOptions entity,
        CancellationToken cancellationToken = default)
    {
        var log = await syncLogPersistence.TryAcquireLogEntry(entity, cancellationToken);

        var trigger = new SyncTrigger
        {
            PriorSyncInfo = new PriorSyncInfo // TODO: replace with actual sync info retrieval logic
            {
                LastSyncLeasedAt = null,
                LastSyncCompletedAt = null,
                LastSyncQueuedAt = null,
                HighWaterMark = null,
            },
            TriggerType = triggerType,
        };

        var isEligibleToSync = triggerType == SyncTriggerType.Manual
                               || await entity.Eligibility.IsEligibleToSync(trigger, entity, cancellationToken);

        if (!isEligibleToSync)
        {
            logger.LogDebug(
                "Entity {EntityName} is not eligible for sync based on the current trigger type {TriggerType}.",
                entity.Entity, triggerType);
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
                return;
            }

            logger.LogInformation("Sync for entity {EntityName} completed successfully.", entity.Entity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during sync for entity {EntityName} with trigger type {TriggerType}.",
                entity.Entity, triggerType);
        }

        // TODO: persist the result of the sync operation
    }
}
