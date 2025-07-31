using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Syncerbell;

/// <summary>
/// An in-memory implementation of ISyncLogPersistence for testing or lightweight scenarios.
/// Not suitable for most production use due to lack of durability.
/// </summary>
public class InMemorySyncLogPersistence(
    SyncerbellOptions options,
    ILogger<InMemorySyncLogPersistence> logger)
    : ISyncLogPersistence
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly List<InMemoryEntry> entries = [];

    /// <inheritdoc />
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public Task<AcquireLogEntryResult?> TryAcquireLogEntry(SyncTriggerType triggerType,
        SyncEntityOptions entity,
        CancellationToken cancellationToken = default)
    {
        var parametersJson = ParameterSerialization.Serialize(entity.Parameters);

        try
        {
            _lock.EnterWriteLock();

            var logEntry = entries
                .SingleOrDefault(e => e.Entity == entity.Entity
                                      && e.ParametersJson == parametersJson
                                      && e.SchemaVersion == entity.SchemaVersion
                                      && e.SyncStatus is SyncStatus.Pending or SyncStatus.InProgress);

            if (!TryProcessExistingEntry(logEntry))
            {
                return Task.FromResult<AcquireLogEntryResult?>(null);
            }

            var priorSyncInfo = GetPriorSyncInfo(entity.Entity, parametersJson, entity.SchemaVersion);

            if (logEntry == null)
            {
                logEntry = new InMemoryEntry
                {
                    Entity = entity.Entity,
                    ParametersJson = parametersJson,
                    SyncStatus = SyncStatus.Pending,
                    SchemaVersion = entity.SchemaVersion,
                    TriggerType = triggerType,
                    CreatedAt = DateTime.UtcNow,
                };
                entries.Add(logEntry);
            }

            UpdateLogEntryLease(logEntry, entity.LeaseExpiration ?? options.DefaultLeaseExpiration);

            // Clone the log entry so that it can't be modified outside the writer lock
            return Task.FromResult<AcquireLogEntryResult?>(new AcquireLogEntryResult(logEntry.Clone(), priorSyncInfo));
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <inheritdoc />
    public Task<AcquireLogEntryResult?> TryAcquireLogEntry(ISyncLogEntry logEntry, SyncEntityOptions entity, CancellationToken cancellationToken = default)
    {
        try
        {
            _lock.EnterWriteLock();

            if (logEntry is not InMemoryEntry inMemoryEntry)
            {
                throw new ArgumentException($"Log entry must be of type {nameof(InMemoryEntry)}", nameof(logEntry));
            }

            // check if the log entry matches the entity and schema version
            if (!logEntry.Entity.Equals(entity.Entity) ||
                logEntry.SchemaVersion != entity.SchemaVersion ||
                logEntry.ParametersJson != ParameterSerialization.Serialize(entity.Parameters))
            {
                logger.LogWarning("Log entry {LogEntryId} does not match entity {Entity}, parameters, or schema version. Cannot acquire.", logEntry.Id, entity.Entity);
                return Task.FromResult<AcquireLogEntryResult?>(null);
            }

            if (!TryProcessExistingEntry(inMemoryEntry))
            {
                return Task.FromResult<AcquireLogEntryResult?>(null);
            }

            var priorSyncInfo = GetPriorSyncInfo(logEntry.Entity, logEntry.ParametersJson, logEntry.SchemaVersion);

            UpdateLogEntryLease(inMemoryEntry, entity.LeaseExpiration ?? options.DefaultLeaseExpiration);

            // Clone the log entry so that it can't be modified outside the writer lock
            return Task.FromResult<AcquireLogEntryResult?>(new AcquireLogEntryResult(inMemoryEntry.Clone(), priorSyncInfo));
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    private static bool TryProcessExistingEntry(InMemoryEntry? logEntry)
    {
        if (logEntry?.LeaseExpiresAt != null && logEntry.LeaseExpiresAt < DateTime.UtcNow)
        {
            // expired lease, set as expired and allow re-acquisition
            logEntry.SyncStatus = SyncStatus.LeaseExpired;
            return true;
        }

        if (logEntry?.LeasedAt != null)
        {
            // If the log entry is already leased or in progress, return false as we can't acquire it yet.
            return false;
        }

        return true;
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    private PriorSyncInfo GetPriorSyncInfo(string entity, string? parametersJson, int? schemaVersion)
    {
        var priorEntriesQuery = entries
            .Where(e => e.Entity == entity
                        && e.ParametersJson == parametersJson
                        && e.SchemaVersion == schemaVersion
                        && e.SyncStatus != SyncStatus.Pending
                        && e.SyncStatus != SyncStatus.InProgress)
            .OrderByDescending(e => e.CreatedAt);

        return new PriorSyncInfo
        {
            HighWaterMark = priorEntriesQuery.FirstOrDefault(i => i.HighWaterMark != null)?.HighWaterMark,
            LastSyncCompletedAt = priorEntriesQuery.FirstOrDefault(i => i.FinishedAt != null)?.FinishedAt,
            LastSyncLeasedAt = priorEntriesQuery.FirstOrDefault(i => i.LeasedAt != null)?.LeasedAt,
            LastSyncQueuedAt = priorEntriesQuery.FirstOrDefault()?.CreatedAt,
        };
    }

    private void UpdateLogEntryLease(InMemoryEntry logEntry, TimeSpan leaseExpiration)
    {
        logEntry.LeasedAt = DateTime.UtcNow;
        logEntry.LeasedBy = options.MachineIdProvider();
        logEntry.LeaseExpiresAt = DateTime.UtcNow.Add(leaseExpiration);
    }

    /// <inheritdoc />
    public Task<ISyncLogEntry?> FindById(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            _lock.EnterReadLock();

            var entry = entries.FirstOrDefault(e => e.Id == id);
            return Task.FromResult<ISyncLogEntry?>(entry?.Clone());
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <inheritdoc />
    public Task UpdateLogEntry(ISyncLogEntry logEntry, CancellationToken cancellationToken = default)
    {
        if (logEntry is not InMemoryEntry entry)
        {
            throw new ArgumentException($"Log entry must be of type {nameof(InMemoryEntry)}", nameof(logEntry));
        }

        try
        {
            _lock.EnterWriteLock();

            var existingEntryIndex = entries.FindIndex(e => e.Id == logEntry.Id);

            if (existingEntryIndex < 0)
            {
                throw new InvalidOperationException($"Log entry with identifier '{logEntry.Id}' not found for update.");
            }

            Debug.Assert(!ReferenceEquals(entries[existingEntryIndex], entry));

            // Replace the existing entry with a clone of the updated one
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
        public InMemoryEntry()
        {
            _id = Guid.NewGuid();
        }

        private InMemoryEntry(Guid id)
        {
            _id = id;
        }

        private readonly Guid _id;

        public required string Entity { get; init; }

        public string? ParametersJson { get; init; }

        public int? SchemaVersion { get; init; }

        public SyncTriggerType TriggerType { get; set; }

        public SyncStatus SyncStatus { get; set; }

        public DateTime CreatedAt { get; init; }

        public DateTime? LeasedAt { get; set; }

        public string? LeasedBy { get; set; }

        public string? QueueMessageId { get; set; }

        public DateTime? QueuedAt { get; set; }

        public DateTime? LeaseExpiresAt { get; set; }

        public DateTime? FinishedAt { get; set; }

        public string? ResultMessage { get; set; }

        public string? HighWaterMark { get; set; }

        public int? ProgressValue { get; set; }

        public int? ProgressMax { get; set; }

        public int? RecordCount { get; set; }

        public string Id => _id.ToString();

        public InMemoryEntry Clone()
        {
            return new InMemoryEntry(_id)
            {
                Entity = Entity,
                ParametersJson = ParametersJson,
                SchemaVersion = SchemaVersion,
                TriggerType = TriggerType,
                SyncStatus = SyncStatus,
                CreatedAt = CreatedAt,
                LeasedAt = LeasedAt,
                LeasedBy = LeasedBy,
                QueueMessageId = QueueMessageId,
                QueuedAt = QueuedAt,
                LeaseExpiresAt = LeaseExpiresAt,
                FinishedAt = FinishedAt,
                ResultMessage = ResultMessage,
                HighWaterMark = HighWaterMark,
                ProgressValue = ProgressValue,
                ProgressMax = ProgressMax,
                RecordCount = RecordCount
            };
        }
    }
}
