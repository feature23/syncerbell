using Microsoft.EntityFrameworkCore;
using Syncerbell.EntityFrameworkCore;

namespace Syncerbell.Tests;

public class SyncLogDbContextTests
{
    private static SyncLogDbContext CreateInMemoryContext(string? schema = null, string? tableName = null)
    {
        var options = new DbContextOptionsBuilder<SyncLogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString("n"))
            .Options;

        var efOptions = new EntityFrameworkCoreSupportOptions
        {
            Schema = schema ?? "dbo",
            SyncLogEntriesTableName = tableName ?? "SyncLogEntries"
        };

        return new SyncLogDbContext(options, efOptions);
    }

    private static SyncLogEntry CreateTestSyncLogEntry(
        string entity = "TestEntity",
        SyncTriggerType triggerType = SyncTriggerType.Manual,
        SyncStatus status = SyncStatus.Pending,
        string? parametersJson = null,
        int? schemaVersion = null)
    {
        return new SyncLogEntry
        {
            Entity = entity,
            TriggerType = triggerType,
            SyncStatus = status,
            ParametersJson = parametersJson,
            SchemaVersion = schemaVersion,
            RowVersion = [], // HACK: this is needed just for the in-memory provider to work correctly
        };
    }

    [Fact]
    public void SyncLogDbContext_CanBeCreated()
    {
        // Arrange & Act
        using var context = CreateInMemoryContext();

        // Assert
        Assert.NotNull(context);
        Assert.NotNull(context.SyncLogEntries);
    }

    [Fact]
    public void SyncLogDbContext_CanConfigureCustomSchemaAndTableName()
    {
        // Arrange
        const string customSchema = "custom";
        const string customTableName = "CustomSyncLogs";

        // Act
        using var context = CreateInMemoryContext(customSchema, customTableName);

        // Assert
        Assert.NotNull(context);
        // Note: In-memory provider doesn't enforce schema/table names, but we can verify the context is created
        Assert.NotNull(context.SyncLogEntries);
    }

    [Fact]
    public async Task SyncLogEntries_CanAddAndRetrieve()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var entry = CreateTestSyncLogEntry();

        // Act
        context.SyncLogEntries.Add(entry);
        await context.SaveChangesAsync();

        var retrievedEntry = await context.SyncLogEntries.FirstOrDefaultAsync();

        // Assert
        Assert.NotNull(retrievedEntry);
        Assert.Equal(entry.Entity, retrievedEntry.Entity);
        Assert.Equal(entry.TriggerType, retrievedEntry.TriggerType);
        Assert.Equal(entry.SyncStatus, retrievedEntry.SyncStatus);
        Assert.True(retrievedEntry.Id > 0); // Should have auto-generated ID
    }

    [Fact]
    public async Task SyncLogEntries_CanQueryByEntity()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var entry1 = CreateTestSyncLogEntry("Entity1");
        var entry2 = CreateTestSyncLogEntry("Entity2");
        var entry3 = CreateTestSyncLogEntry("Entity1");

        context.SyncLogEntries.AddRange(entry1, entry2, entry3);
        await context.SaveChangesAsync();

        // Act
        var entity1Entries = await context.SyncLogEntries
            .Where(e => e.Entity == "Entity1")
            .ToListAsync();

        // Assert
        Assert.Equal(2, entity1Entries.Count);
        Assert.All(entity1Entries, e => Assert.Equal("Entity1", e.Entity));
    }

    [Fact]
    public async Task SyncLogEntries_CanQueryByStatus()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var pendingEntry = CreateTestSyncLogEntry(status: SyncStatus.Pending);
        var inProgressEntry = CreateTestSyncLogEntry(status: SyncStatus.InProgress);
        var completedEntry = CreateTestSyncLogEntry(status: SyncStatus.Completed);

        context.SyncLogEntries.AddRange(pendingEntry, inProgressEntry, completedEntry);
        await context.SaveChangesAsync();

        // Act
        var pendingEntries = await context.SyncLogEntries
            .Where(e => e.SyncStatus == SyncStatus.Pending)
            .ToListAsync();

        // Assert
        Assert.Single(pendingEntries);
        Assert.Equal(SyncStatus.Pending, pendingEntries[0].SyncStatus);
    }

    [Fact]
    public async Task SyncLogEntries_CanUpdateStatus()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var entry = CreateTestSyncLogEntry();

        context.SyncLogEntries.Add(entry);
        await context.SaveChangesAsync();

        // Act
        entry.SyncStatus = SyncStatus.InProgress;
        await context.SaveChangesAsync();

        var updatedEntry = await context.SyncLogEntries.FindAsync(entry.Id);

        // Assert
        Assert.NotNull(updatedEntry);
        Assert.Equal(SyncStatus.InProgress, updatedEntry.SyncStatus);
    }

    [Fact]
    public async Task SyncLogEntries_CanUpdateLeaseInformation()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var entry = CreateTestSyncLogEntry();
        var leasedAt = DateTime.UtcNow;
        var leaseExpiresAt = leasedAt.AddMinutes(30);
        const string leasedBy = "TestWorker";

        context.SyncLogEntries.Add(entry);
        await context.SaveChangesAsync();

        // Act
        entry.LeasedAt = leasedAt;
        entry.LeaseExpiresAt = leaseExpiresAt;
        entry.LeasedBy = leasedBy;
        await context.SaveChangesAsync();

        var updatedEntry = await context.SyncLogEntries.FindAsync(entry.Id);

        // Assert
        Assert.NotNull(updatedEntry);
        Assert.Equal(leasedAt, updatedEntry.LeasedAt);
        Assert.Equal(leaseExpiresAt, updatedEntry.LeaseExpiresAt);
        Assert.Equal(leasedBy, updatedEntry.LeasedBy);
    }

    [Fact]
    public async Task SyncLogEntries_CanUpdateProgressInformation()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var entry = CreateTestSyncLogEntry();

        context.SyncLogEntries.Add(entry);
        await context.SaveChangesAsync();

        // Act
        entry.ProgressValue = 50;
        entry.ProgressMax = 100;
        entry.RecordCount = 250;
        entry.HighWaterMark = "12345";
        await context.SaveChangesAsync();

        var updatedEntry = await context.SyncLogEntries.FindAsync(entry.Id);

        // Assert
        Assert.NotNull(updatedEntry);
        Assert.Equal(50, updatedEntry.ProgressValue);
        Assert.Equal(100, updatedEntry.ProgressMax);
        Assert.Equal(250, updatedEntry.RecordCount);
        Assert.Equal("12345", updatedEntry.HighWaterMark);
    }

    [Fact]
    public async Task SyncLogEntries_CanUpdateQueueInformation()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var entry = CreateTestSyncLogEntry();
        var queuedAt = DateTime.UtcNow;
        const string queueMessageId = "msg-12345";

        context.SyncLogEntries.Add(entry);
        await context.SaveChangesAsync();

        // Act
        entry.QueuedAt = queuedAt;
        entry.QueueMessageId = queueMessageId;
        await context.SaveChangesAsync();

        var updatedEntry = await context.SyncLogEntries.FindAsync(entry.Id);

        // Assert
        Assert.NotNull(updatedEntry);
        Assert.Equal(queuedAt, updatedEntry.QueuedAt);
        Assert.Equal(queueMessageId, updatedEntry.QueueMessageId);
    }

    [Fact]
    public async Task SyncLogEntries_CanUpdateCompletionInformation()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var entry = CreateTestSyncLogEntry();
        var finishedAt = DateTime.UtcNow;
        const string resultMessage = "Sync completed successfully";

        context.SyncLogEntries.Add(entry);
        await context.SaveChangesAsync();

        // Act
        entry.FinishedAt = finishedAt;
        entry.ResultMessage = resultMessage;
        entry.SyncStatus = SyncStatus.Completed;
        await context.SaveChangesAsync();

        var updatedEntry = await context.SyncLogEntries.FindAsync(entry.Id);

        // Assert
        Assert.NotNull(updatedEntry);
        Assert.Equal(finishedAt, updatedEntry.FinishedAt);
        Assert.Equal(resultMessage, updatedEntry.ResultMessage);
        Assert.Equal(SyncStatus.Completed, updatedEntry.SyncStatus);
    }

    [Fact]
    public async Task SyncLogEntries_CanDeleteEntry()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var entry = CreateTestSyncLogEntry();

        context.SyncLogEntries.Add(entry);
        await context.SaveChangesAsync();
        var entryId = entry.Id;

        // Act
        context.SyncLogEntries.Remove(entry);
        await context.SaveChangesAsync();

        var deletedEntry = await context.SyncLogEntries.FindAsync(entryId);

        // Assert
        Assert.Null(deletedEntry);
    }

    [Fact]
    public async Task SyncLogEntries_CanQueryWithComplexFilters()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var entries = new[]
        {
            CreateTestSyncLogEntry("Entity1"),
            CreateTestSyncLogEntry("Entity1", SyncTriggerType.Timer, SyncStatus.InProgress),
            CreateTestSyncLogEntry("Entity2", SyncTriggerType.Manual, SyncStatus.Completed),
            CreateTestSyncLogEntry("Entity2", SyncTriggerType.Timer)
        };

        context.SyncLogEntries.AddRange(entries);
        await context.SaveChangesAsync();

        // Act - Find all Entity1 entries that are not completed
        var filteredEntries = await context.SyncLogEntries
            .Where(e => e.Entity == "Entity1" && e.SyncStatus != SyncStatus.Completed)
            .OrderBy(e => e.TriggerType)
            .ToListAsync();

        // Assert
        Assert.Equal(2, filteredEntries.Count);
        Assert.All(filteredEntries, e => Assert.Equal("Entity1", e.Entity));
        Assert.All(filteredEntries, e => Assert.NotEqual(SyncStatus.Completed, e.SyncStatus));
        Assert.Equal(SyncTriggerType.Timer, filteredEntries[0].TriggerType);
        Assert.Equal(SyncTriggerType.Manual, filteredEntries[1].TriggerType);
    }

    [Fact]
    public async Task SyncLogEntries_WithParametersAndSchemaVersion()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        const string parametersJson = """{"param1": "value1", "param2": 42}""";
        const int schemaVersion = 2;
        var entry = CreateTestSyncLogEntry(
            parametersJson: parametersJson,
            schemaVersion: schemaVersion);

        // Act
        context.SyncLogEntries.Add(entry);
        await context.SaveChangesAsync();

        var retrievedEntry = await context.SyncLogEntries.FirstAsync();

        // Assert
        Assert.Equal(parametersJson, retrievedEntry.ParametersJson);
        Assert.Equal(schemaVersion, retrievedEntry.SchemaVersion);
    }

    [Fact]
    public async Task SyncLogEntries_ISyncLogEntryInterface_IdProperty()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var entry = CreateTestSyncLogEntry();

        context.SyncLogEntries.Add(entry);
        await context.SaveChangesAsync();

        // Act
        var retrievedEntry = await context.SyncLogEntries.FirstAsync();
        var interfaceEntry = (ISyncLogEntry)retrievedEntry;

        // Assert
        Assert.Equal(retrievedEntry.Id.ToString(), interfaceEntry.Id);
    }
}
