using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Syncerbell.EntityFrameworkCore;

/// <summary>
/// Provides an Entity Framework Core-based implementation of <see cref="ISyncLogPersistence"/> for durable sync log storage.
/// </summary>
public class EntityFrameworkCoreSyncLogPersistence(
    SyncLogDbContext context,
    SyncerbellOptions options,
    ILogger<EntityFrameworkCoreSyncLogPersistence> logger)
    : ISyncLogPersistence
{
    /// <inheritdoc />
    public async Task<AcquireLogEntryResult?> TryAcquireLogEntry(SyncTriggerType triggerType,
        SyncEntityOptions entity,
        AcquireLeaseBehavior behavior = AcquireLeaseBehavior.AcquireIfNotLeased,
        CancellationToken cancellationToken = default)
    {
        return await ResilientTransaction.New(context).ExecuteAsync(async () =>
        {
            var parametersJson = ParameterSerialization.Serialize(entity.Parameters);

            var logEntry = await GetPendingOrInProgressLogEntry(entity, parametersJson, cancellationToken);

            if (logEntry?.LeaseExpiresAt != null && logEntry.LeaseExpiresAt < DateTime.UtcNow)
            {
                // expired lease, set as expired and allow re-acquisition
                logger.LogWarning(
                    "Lease expired for log entry {LogEntryId} for entity {Entity}. Setting status to LeaseExpired.",
                    logEntry.Id, entity.Entity);

                logEntry.SyncStatus = SyncStatus.LeaseExpired;
                await context.SaveChangesAsync(cancellationToken);

                logEntry = null; // reset logEntry to allow creation of a new one
            }
            else if (behavior != AcquireLeaseBehavior.ForceAcquire && logEntry?.LeasedAt != null)
            {
                // If the log entry is already leased or in progress, return null as we can't acquire it yet.
                // This could either be a pending or in-progress entry.
                return null;
            }

            var priorSyncInfo = await GetPriorSyncInfo(entity.Entity, parametersJson, entity.SchemaVersion, cancellationToken);

            logEntry = await CreateOrUpdateLogEntry(behavior, triggerType, entity, parametersJson, logEntry, cancellationToken);

            return new AcquireLogEntryResult(logEntry, priorSyncInfo);
        }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AcquireLogEntryResult?> TryAcquireLogEntry(ISyncLogEntry syncLogEntry,
        SyncEntityOptions entity,
        AcquireLeaseBehavior behavior = AcquireLeaseBehavior.AcquireIfNotLeased,
        CancellationToken cancellationToken = default)
    {
        return await ResilientTransaction.New(context).ExecuteAsync(async () =>
        {
            if (syncLogEntry is not SyncLogEntry efSyncLogEntry)
            {
                throw new ArgumentException($"Log entry must be of type {nameof(SyncLogEntry)}", nameof(syncLogEntry));
            }

            // check if the log entry matches the entity and schema version
            if (!syncLogEntry.Entity.Equals(entity.Entity) ||
                syncLogEntry.SchemaVersion != entity.SchemaVersion ||
                syncLogEntry.ParametersJson != ParameterSerialization.Serialize(entity.Parameters))
            {
                logger.LogWarning("Log entry {LogEntryId} does not match entity {Entity}, parameters, or schema version. Cannot acquire.", syncLogEntry.Id, entity.Entity);
                return null;
            }

            // Check if lease is expired
            if (syncLogEntry.LeaseExpiresAt != null && syncLogEntry.LeaseExpiresAt < DateTime.UtcNow)
            {
                // expired lease, set as expired and allow re-acquisition
                syncLogEntry.SyncStatus = SyncStatus.LeaseExpired;
                await context.SaveChangesAsync(cancellationToken);
            }
            else if (behavior != AcquireLeaseBehavior.ForceAcquire && syncLogEntry.LeasedAt != null)
            {
                // If the log entry is already leased or in progress, return null as we can't acquire it yet.
                return null;
            }

            // Get prior sync info using the log entry data directly
            var priorSyncInfo = await GetPriorSyncInfo(syncLogEntry.Entity, syncLogEntry.ParametersJson, syncLogEntry.SchemaVersion, cancellationToken);

            // Lease the log entry
            if (behavior != AcquireLeaseBehavior.DoNotAcquire)
            {
                UpdateLogEntryLease(efSyncLogEntry, entity.LeaseExpiration ?? options.DefaultLeaseExpiration);
            }

            context.SyncLogEntries.Update(efSyncLogEntry);
            await context.SaveChangesAsync(cancellationToken);

            return new AcquireLogEntryResult(efSyncLogEntry, priorSyncInfo);
        }, cancellationToken);
    }

    private async Task<PriorSyncInfo> GetPriorSyncInfo(string entity, string? parametersJson, int? schemaVersion,
        CancellationToken cancellationToken)
    {
        var priorEntriesQuery = context.SyncLogEntries
            .Where(e => e.Entity == entity && e.ParametersJson == parametersJson && e.SchemaVersion == schemaVersion)
            .OrderByDescending(e => e.CreatedAt);

        return new PriorSyncInfo
        {
            HighWaterMark = (await priorEntriesQuery.FirstOrDefaultAsync(i => i.HighWaterMark != null, cancellationToken))?.HighWaterMark,
            LastSyncQueuedAt = (await priorEntriesQuery.FirstOrDefaultAsync(cancellationToken))?.CreatedAt,
            LastSyncLeasedAt = (await priorEntriesQuery.FirstOrDefaultAsync(i => i.LeasedAt != null, cancellationToken))?.LeasedAt,
            LastSyncCompletedAt = (await priorEntriesQuery.FirstOrDefaultAsync(i => i.FinishedAt != null, cancellationToken))?.FinishedAt,
        };
    }

    private async Task<SyncLogEntry?> GetPendingOrInProgressLogEntry(SyncEntityOptions entity, string? parametersJson,
        CancellationToken cancellationToken)
    {
        return await context.SyncLogEntries
            .Where(e => e.Entity == entity.Entity
                        && e.ParametersJson == parametersJson
                        && e.SchemaVersion == entity.SchemaVersion
                        && (e.SyncStatus == SyncStatus.Pending || e.SyncStatus == SyncStatus.InProgress))
            .SingleOrDefaultAsync(cancellationToken);
    }

    private void UpdateLogEntryLease(SyncLogEntry logEntry, TimeSpan leaseExpiration)
    {
        logEntry.LeasedAt = DateTime.UtcNow;
        logEntry.LeasedBy = options.MachineIdProvider();
        logEntry.LeaseExpiresAt = DateTime.UtcNow.Add(leaseExpiration);
    }

    private async Task<SyncLogEntry> CreateOrUpdateLogEntry(
        AcquireLeaseBehavior behavior,
        SyncTriggerType triggerType,
        SyncEntityOptions entity,
        string? parametersJson,
        SyncLogEntry? logEntry,
        CancellationToken cancellationToken)
    {
        if (logEntry == null)
        {
            logEntry = new SyncLogEntry
            {
                Entity = entity.Entity,
                ParametersJson = parametersJson,
                SchemaVersion = entity.SchemaVersion,
                TriggerType = triggerType,
                SyncStatus = SyncStatus.Pending,
                CreatedAt = DateTime.UtcNow,
            };
            context.SyncLogEntries.Add(logEntry);
            await context.SaveChangesAsync(cancellationToken);
        }
        else if (behavior != AcquireLeaseBehavior.DoNotAcquire)
        {
            UpdateLogEntryLease(logEntry, entity.LeaseExpiration ?? options.DefaultLeaseExpiration);
            context.SyncLogEntries.Update(logEntry);
            await context.SaveChangesAsync(cancellationToken);
        }

        return logEntry;
    }

    /// <inheritdoc />
    public async Task<ISyncLogEntry?> FindById(string id, CancellationToken cancellationToken = default)
    {
        if (!int.TryParse(id, out var intId))
        {
            throw new ArgumentException($"Identifier '{id}' is not a valid integer.", nameof(id));
        }

        return await context.SyncLogEntries
            .FirstOrDefaultAsync(e => e.Id == intId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateLogEntry(ISyncLogEntry logEntry, CancellationToken cancellationToken = default)
    {
        if (logEntry is not SyncLogEntry entry)
        {
            throw new ArgumentException($"Log entry must be of type {nameof(SyncLogEntry)}", nameof(logEntry));
        }

        context.SyncLogEntries.Update(entry);
        await context.SaveChangesAsync(cancellationToken);
    }
}
