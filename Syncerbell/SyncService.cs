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
        var entities = await GetAllEntities(cancellationToken);

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
        var acquireResult = await syncLogPersistence.TryAcquireLogEntry(triggerType, entity, cancellationToken);

        if (acquireResult is not { SyncLogEntry: { } log, PriorSyncInfo: { } priorSyncInfo })
        {
            // If no log entry was acquired, we skip the sync for this entity.
            // This could be because the entity is already being processed or has a pending sync.
            logger.LogInformation("No log entry acquired for entity {EntityName}. Skipping sync.", entity.Entity);
            return null;
        }

        return await ProcessSyncLogEntry(log, priorSyncInfo, triggerType, entity, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<SyncResult?> SyncEntityIfEligible(string syncLogEntryId,
        SyncTriggerType triggerType,
        CancellationToken cancellationToken = default)
    {
        var log = await syncLogPersistence.FindById(syncLogEntryId, cancellationToken);

        if (log is null)
        {
            throw new InvalidOperationException($"Sync log entry with identifier '{syncLogEntryId}' not found.");
        }

        var allEntities = await GetAllEntities(cancellationToken);

        var entity = allEntities.FirstOrDefault(e => e.Entity == log.Entity && e.SchemaVersion == log.SchemaVersion && e.ParametersJson == log.ParametersJson)
                     ?? throw new InvalidOperationException($"No entity configuration found for entity {log.Entity} " +
                                                            $"with parameters {log.ParametersJson ?? "null"} " +
                                                            $"and schema version {log.SchemaVersion?.ToString() ?? "null"} " +
                                                            $"from log entry {syncLogEntryId}.");

        var acquireResult = await syncLogPersistence.TryAcquireLogEntry(log, entity, cancellationToken);

        if (acquireResult is not { PriorSyncInfo: { } priorSyncInfo })
        {
            // If no log entry was acquired, it might not exist or is already being processed
            logger.LogInformation("No log entry acquired for ID {SyncLogEntryId}. Entry may not exist or is already being processed.", syncLogEntryId);
            return null;
        }

        return await ProcessSyncLogEntry(log, priorSyncInfo, triggerType, entity, cancellationToken);
    }

    /// <summary>
    /// Gets all entities configured for synchronization, including both statically configured and dynamically provided entities.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A list of all sync entity options.</returns>
    private async Task<List<SyncEntityOptions>> GetAllEntities(CancellationToken cancellationToken)
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

        return entities;
    }

    /// <summary>
    /// Processes a sync log entry by checking eligibility and executing the sync if eligible.
    /// </summary>
    /// <param name="log">The sync log entry to process.</param>
    /// <param name="priorSyncInfo">The prior sync information.</param>
    /// <param name="triggerType">The trigger type for the sync operation.</param>
    /// <param name="entity">The entity configuration.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>Returns a task that resolves to a <see cref="SyncResult"/> object indicating the outcome of the sync operation, or null if not eligible.</returns>
    private async Task<SyncResult?> ProcessSyncLogEntry(ISyncLogEntry log,
        PriorSyncInfo priorSyncInfo,
        SyncTriggerType triggerType,
        SyncEntityOptions entity,
        CancellationToken cancellationToken)
    {
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

        await syncLogPersistence.UpdateLogEntry(logEntry, cancellationToken);
    }

    private async Task UpdateLogEntry(ISyncLogEntry log, SyncStatus status, SyncResult syncResult, CancellationToken cancellationToken)
    {
        log.SyncStatus = status;
        log.ResultMessage = syncResult.Message ?? (syncResult.Success ? SyncSuccessMessage : SyncFailedMessage);
        log.FinishedAt = DateTime.UtcNow;
        log.HighWaterMark = syncResult.HighWaterMark;
        await syncLogPersistence.UpdateLogEntry(log, cancellationToken);
    }
}
