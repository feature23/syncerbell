using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Syncerbell.EntityFrameworkCore;

/// <summary>
/// Provides an Entity Framework Core-based implementation of <see cref="ISyncLogPersistence"/> for durable sync log storage.
/// </summary>
public class EntityFrameworkCoreSyncLogPersistence(
    SyncLogDbContext context,
    SyncerbellOptions options)
    : ISyncLogPersistence
{
    /// <summary>
    /// Attempts to acquire a log entry for the specified entity, creating or updating an entry as needed.
    /// </summary>
    /// <param name="entity">The entity options for which to acquire a log entry.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="AcquireLogEntryResult"/> containing the acquired log entry and prior sync info, or <c>null</c> if the entry is already leased.</returns>
    public async Task<AcquireLogEntryResult?> TryAcquireLogEntry(SyncEntityOptions entity, CancellationToken cancellationToken = default)
    {
        return await ResilientTransaction.New(context).ExecuteAsync(async () =>
        {
            var parametersJson = entity.Parameters != null
                ? JsonSerializer.Serialize(entity.Parameters)
                : null;

            var logEntry = await GetPendingOrInProgressLogEntry(entity, parametersJson, cancellationToken);

            if (logEntry?.LeaseExpiresAt != null && logEntry.LeaseExpiresAt < DateTime.UtcNow)
            {
                // expired lease, set as expired and allow re-acquisition
                logEntry.SyncStatus = SyncStatus.LeaseExpired;

                await context.SaveChangesAsync(cancellationToken);

                logEntry = null; // reset logEntry to allow creation of a new one
            }
            else if (logEntry?.LeasedAt != null)
            {
                // If the log entry is already leased or in progress, return null as we can't acquire it yet.
                // This could either be a pending or in-progress entry.
                return null;
            }

            var priorSyncInfo = await GetPriorSyncInfo(entity, parametersJson, cancellationToken);

            logEntry = await CreateOrUpdateLogEntry(entity, parametersJson, logEntry, cancellationToken);

            return new AcquireLogEntryResult(logEntry, priorSyncInfo);
        }, cancellationToken);
    }

    private async Task<PriorSyncInfo> GetPriorSyncInfo(SyncEntityOptions entity, string? parametersJson, CancellationToken cancellationToken)
    {
        var priorEntriesQuery = context.SyncLogEntries
            .Where(e => e.Entity == entity.Entity && e.ParametersJson == parametersJson && e.SchemaVersion == entity.SchemaVersion)
            .OrderByDescending(e => e.CreatedAt);

        return new PriorSyncInfo
        {
            HighWaterMark = (await priorEntriesQuery.FirstOrDefaultAsync(i => i.HighWaterMark != null, cancellationToken))?.HighWaterMark,
            LastSyncQueuedAt = (await priorEntriesQuery.FirstOrDefaultAsync(cancellationToken))?.CreatedAt,
            LastSyncLeasedAt = (await priorEntriesQuery.FirstOrDefaultAsync(i => i.LeasedAt != null, cancellationToken))?.LeasedAt,
            LastSyncCompletedAt = (await priorEntriesQuery.FirstOrDefaultAsync(i => i.FinishedAt != null, cancellationToken))?.FinishedAt,
        };
    }

    private async Task<SyncLogEntry?> GetPendingOrInProgressLogEntry(SyncEntityOptions entity, string? parametersJson, CancellationToken cancellationToken)
    {
        return await context.SyncLogEntries
            .Where(e => e.Entity == entity.Entity
                        && e.ParametersJson == parametersJson
                        && e.SchemaVersion == entity.SchemaVersion
                        && (e.SyncStatus == SyncStatus.Pending || e.SyncStatus == SyncStatus.InProgress))
            .SingleOrDefaultAsync(cancellationToken);
    }

    private async Task<SyncLogEntry> CreateOrUpdateLogEntry(SyncEntityOptions entity,
        string? parametersJson, SyncLogEntry? logEntry, CancellationToken cancellationToken)
    {
        if (logEntry == null)
        {
            logEntry = new SyncLogEntry
            {
                Entity = entity.Entity,
                ParametersJson = parametersJson,
                SchemaVersion = entity.SchemaVersion,
                SyncStatus = SyncStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                LeasedAt = DateTime.UtcNow,
                LeasedBy = options.MachineIdProvider(),
                LeaseExpiresAt = DateTime.UtcNow.Add(entity.LeaseExpiration ?? options.DefaultLeaseExpiration),
            };
            context.SyncLogEntries.Add(logEntry);
        }
        else
        {
            logEntry.LeasedAt = DateTime.UtcNow;
            logEntry.LeasedBy = options.MachineIdProvider();
            logEntry.LeaseExpiresAt = DateTime.UtcNow.Add(entity.LeaseExpiration ?? options.DefaultLeaseExpiration);
            context.SyncLogEntries.Update(logEntry);
        }

        await context.SaveChangesAsync(cancellationToken);

        return logEntry;
    }

    /// <summary>
    /// Updates an existing log entry for the specified entity.
    /// </summary>
    /// <param name="entity">The entity options associated with the log entry.</param>
    /// <param name="logEntry">The log entry to update. Must be of type <see cref="SyncLogEntry"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="logEntry"/> is not of type <see cref="SyncLogEntry"/>.</exception>
    /// <returns>A completed <see cref="Task"/>.</returns>
    public async Task UpdateLogEntry(SyncEntityOptions entity, ISyncLogEntry logEntry, CancellationToken cancellationToken = default)
    {
        if (logEntry is not SyncLogEntry entry)
            throw new ArgumentException($"Log entry must be of type {nameof(SyncLogEntry)}", nameof(logEntry));

        context.SyncLogEntries.Update(entry);
        await context.SaveChangesAsync(cancellationToken);
    }
}
