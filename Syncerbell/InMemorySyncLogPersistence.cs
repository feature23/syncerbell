using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Syncerbell;

/// <summary>
/// An in-memory implementation of ISyncLogPersistence for testing or lightweight scenarios.
/// Not suitable for most production use due to lack of durability.
/// </summary>
public class InMemorySyncLogPersistence(SyncerbellOptions options) : ISyncLogPersistence
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly List<InMemoryEntry> entries = new();

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public Task<AcquireLogEntryResult?> TryAcquireLogEntry(SyncEntityOptions entity, CancellationToken cancellationToken = default)
    {
        var parametersJson = entity.Parameters != null
            ? System.Text.Json.JsonSerializer.Serialize(entity.Parameters)
            : null;

        try
        {
            _lock.EnterWriteLock();

            var logEntry = entries
                .SingleOrDefault(e => e.Entity == entity.Entity
                                      && e.ParametersJson == parametersJson
                                      && e.SyncStatus is SyncStatus.Pending or SyncStatus.InProgress);

            if (logEntry?.LeaseExpiresAt != null && logEntry.LeaseExpiresAt < DateTime.UtcNow)
            {
                // expired lease, set as expired and allow re-acquisition
                logEntry.SyncStatus = SyncStatus.LeaseExpired;

                logEntry = null; // reset logEntry to allow creation of a new one
            }
            else if (logEntry?.LeasedAt != null)
            {
                // If the log entry is already leased or in progress, return null as we can't acquire it yet.
                // This could either be a pending or in-progress entry.
                return Task.FromResult<AcquireLogEntryResult?>(null);
            }

            var priorEntriesQuery = entries
                .Where(e => e.Entity == entity.Entity && e.ParametersJson == parametersJson && e.SyncStatus != SyncStatus.Pending && e.SyncStatus != SyncStatus.InProgress)
                .OrderByDescending(e => e.CreatedAt);

            var priorSyncInfo = new PriorSyncInfo
            {
                HighWaterMark = priorEntriesQuery.FirstOrDefault(i => i.HighWaterMark != null)?.HighWaterMark,
                LastSyncCompletedAt = priorEntriesQuery.FirstOrDefault(i => i.FinishedAt != null)?.FinishedAt,
                LastSyncLeasedAt = priorEntriesQuery.FirstOrDefault(i => i.LeasedAt != null)?.LeasedAt,
                LastSyncQueuedAt = priorEntriesQuery.FirstOrDefault()?.CreatedAt,
            };

            if (logEntry == null)
            {
                logEntry = new InMemoryEntry
                {
                    Entity = entity.Entity,
                    ParametersJson = parametersJson,
                    SyncStatus = SyncStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    LeasedAt = DateTime.UtcNow,
                    LeasedBy = options.MachineIdProvider(),
                    LeaseExpiresAt = DateTime.UtcNow.Add(entity.LeaseExpiration ?? options.DefaultLeaseExpiration),
                };
                entries.Add(logEntry);
            }
            else
            {
                logEntry.LeasedAt = DateTime.UtcNow;
                logEntry.LeasedBy = options.MachineIdProvider();
                logEntry.LeaseExpiresAt =
                    DateTime.UtcNow.Add(entity.LeaseExpiration ?? options.DefaultLeaseExpiration);
            }

            // Clone the log entry so that it can't be modified outside the writer lock
            return Task.FromResult<AcquireLogEntryResult?>(new AcquireLogEntryResult(logEntry.Clone(), priorSyncInfo));
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public Task UpdateLogEntry(SyncEntityOptions entity, ISyncLogEntry logEntry, CancellationToken cancellationToken = default)
    {
        if (logEntry is not InMemoryEntry entry)
            throw new ArgumentException($"Log entry must be of type {nameof(InMemoryEntry)}", nameof(logEntry));

        try
        {
            _lock.EnterWriteLock();

            var existingEntryIndex = entries.FindIndex(e => e.Entity == entry.Entity && e.ParametersJson == entry.ParametersJson);

            if (existingEntryIndex < 0)
            {
                throw new InvalidOperationException("Log entry not found for update.");
            }

            Debug.Assert(!ReferenceEquals(entries[existingEntryIndex], entry));

            // Replace the existing entry with a clone of the updated one,
            // so that the caller cannot modify it outside the lock after calling this method.
            entries[existingEntryIndex] = entry.Clone();
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        return Task.CompletedTask;
    }

    private class InMemoryEntry : ISyncLogEntry
    {
        public required string Entity { get; init; }

        public string? ParametersJson { get; init; }

        public SyncStatus SyncStatus { get; set; }

        public DateTime CreatedAt { get; init; }

        public DateTime? LeasedAt { get; set; }

        public string? LeasedBy { get; set; }

        public DateTime? FinishedAt { get; set; }

        public string? ResultMessage { get; set; }

        public string? HighWaterMark { get; set; }

        public DateTime? LeaseExpiresAt { get; set; }

        public InMemoryEntry Clone()
        {
            return new InMemoryEntry
            {
                Entity = Entity,
                ParametersJson = ParametersJson,
                SyncStatus = SyncStatus,
                CreatedAt = CreatedAt,
                LeasedAt = LeasedAt,
                LeasedBy = LeasedBy,
                FinishedAt = FinishedAt,
                ResultMessage = ResultMessage,
                HighWaterMark = HighWaterMark,
                LeaseExpiresAt = LeaseExpiresAt
            };
        }
    }
}
