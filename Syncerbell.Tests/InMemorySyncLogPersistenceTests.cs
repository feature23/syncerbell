using Microsoft.Extensions.Logging;

namespace Syncerbell.Tests;

public class InMemorySyncLogPersistenceTests
{
    [Fact]
    public async Task InMemorySyncLogPersistence_ShouldCreateNewLogEntryIfNotFound()
    {
        // Arrange
        var options = new SyncerbellOptions();
        var logger = new LoggerFactory().CreateLogger<InMemorySyncLogPersistence>();
        var persistence = new InMemorySyncLogPersistence(options, logger);
        var entity = new SyncEntityOptions("TestEntity", typeof(TestEntitySync));

        // Act
        var result = await persistence.TryAcquireLogEntry(SyncTriggerType.Manual, entity);

        // Assert
        Assert.NotNull(result);
        var logEntry = result.SyncLogEntry;
        Assert.NotNull(logEntry);
        Assert.NotNull(logEntry.LeasedAt);
        Assert.Equal(SyncStatus.Pending, logEntry.SyncStatus);
        Assert.Equal(options.MachineIdProvider(), logEntry.LeasedBy);
        Assert.NotNull(logEntry.LeaseExpiresAt);
        Assert.True(logEntry.LeaseExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task InMemorySyncLogPersistence_ShouldAcquireExistingLogEntry()
    {
        // Arrange
        var options = new SyncerbellOptions();
        var logger = new LoggerFactory().CreateLogger<InMemorySyncLogPersistence>();
        var persistence = new InMemorySyncLogPersistence(options, logger);
        var entity = new SyncEntityOptions("TestEntity", typeof(TestEntitySync));

        var originalLogEntry = await persistence.TryAcquireLogEntry(SyncTriggerType.Manual, entity);
        Assert.NotNull(originalLogEntry?.SyncLogEntry);
        originalLogEntry.SyncLogEntry.LeaseExpiresAt = null;
        originalLogEntry.SyncLogEntry.LeasedAt = null;
        originalLogEntry.SyncLogEntry.LeasedBy = null;
        await persistence.UpdateLogEntry(originalLogEntry.SyncLogEntry);

        // Act
        var result = await persistence.TryAcquireLogEntry(SyncTriggerType.Manual, entity);

        // Assert
        Assert.NotNull(result);
        var logEntry = result.SyncLogEntry;
        Assert.NotNull(logEntry);
        Assert.NotSame(originalLogEntry, logEntry); // should be a clone
        Assert.NotNull(logEntry.LeasedAt);
        Assert.Equal(SyncStatus.Pending, logEntry.SyncStatus);
        Assert.Equal(options.MachineIdProvider(), logEntry.LeasedBy);
        Assert.NotNull(logEntry.LeaseExpiresAt);
        Assert.True(logEntry.LeaseExpiresAt > DateTime.UtcNow);
    }

    private class TestEntitySync : IEntitySync
    {
        public Task<SyncResult> Run(EntitySyncContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new SyncResult(context.Entity, true));
        }
    }
}
