using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Syncerbell.EntityFrameworkCore;

public class EntityFrameworkCoreSyncLogPersistence(
    SyncLogDbContext context,
    SyncerbellOptions options)
    : ISyncLogPersistence
{
    public async Task<AcquireLogEntryResult> TryAcquireLogEntry(SyncEntityOptions entity, CancellationToken cancellationToken = default)
    {
        return await ResilientTransaction.New(context).ExecuteAsync(async () =>
        {
            var parametersJson = entity.Parameters != null
                ? JsonSerializer.Serialize(entity.Parameters)
                : null;

            var logEntry = await context.SyncLogEntries
                .Where(e => e.Entity == entity.Entity
                            && e.ParametersJson == parametersJson
                            && (e.SyncStatus == SyncStatus.Pending || e.SyncStatus == SyncStatus.InProgress))
                .SingleOrDefaultAsync(cancellationToken);

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
                return new AcquireLogEntryResult(null, null);
            }

            if (logEntry == null)
            {
                logEntry = new SyncLogEntry
                {
                    Entity = entity.Entity,
                    ParametersJson = parametersJson,
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

            return new AcquireLogEntryResult(logEntry, null /* TODO */);
        }, cancellationToken);
    }

    public async Task UpdateLogEntry(SyncEntityOptions entity, ISyncLogEntry logEntry, CancellationToken cancellationToken = default)
    {
        if (logEntry is not SyncLogEntry entry)
            throw new ArgumentException($"Log entry must be of type {nameof(SyncLogEntry)}", nameof(logEntry));

        context.SyncLogEntries.Update(entry);
        await context.SaveChangesAsync(cancellationToken);
    }
}
