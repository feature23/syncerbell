// ReSharper disable ClassNeverInstantiated.Local
namespace Syncerbell.Tests;

public class SyncEntityOptionsTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        const string entityType = "TestEntity";
        var entitySyncType = typeof(DummySync);

        // Act
        var options = new SyncEntityOptions(entityType, entitySyncType);

        // Assert
        Assert.Equal(entityType, options.Entity);
        Assert.Equal(entitySyncType, options.EntitySyncType);

        var strategy = Assert.IsType<IntervalEligibilityStrategy>(options.Eligibility);
        Assert.Equal(TimeSpan.FromDays(1), strategy.Interval);
    }

    [Fact]
    public void Create_ShouldSetSchemaVersion_WhenAttributeIsPresent()
    {
        // Act
        var options = SyncEntityOptions.Create<EntityWithSchemaVersion, DummySync>();

        // Assert
        Assert.Equal(5, options.SchemaVersion);
    }

    [Fact]
    public void Create_ShouldSetSchemaVersionNull_WhenAttributeIsAbsent()
    {
        // Act
        var options = SyncEntityOptions.Create<EntityWithoutSchemaVersion, DummySync>();

        // Assert
        Assert.Null(options.SchemaVersion);
    }

    [Fact]
    public void Create_WithEntityName_ShouldSetEntityNameAndSyncType()
    {
        // Arrange
        const string customEntityName = "CustomEntityName";

        // Act
        var options = SyncEntityOptions.Create<EntityWithoutSchemaVersion, DummySync>(customEntityName);

        // Assert
        Assert.Equal(customEntityName, options.Entity);
        Assert.Equal(typeof(DummySync), options.EntitySyncType);
        Assert.Equal(typeof(EntityWithoutSchemaVersion), options.EntityType);
    }

    [Fact]
    public void Create_Generic_ShouldSetEntityNameFromType()
    {
        // Act
        var options = SyncEntityOptions.Create<EntityWithoutSchemaVersion, DummySync>();

        // Assert
        Assert.Equal(nameof(EntityWithoutSchemaVersion), options.Entity);
        Assert.Equal(typeof(DummySync), options.EntitySyncType);
        Assert.Equal(typeof(EntityWithoutSchemaVersion), options.EntityType);
    }

    [SchemaVersion(5)]
    private class EntityWithSchemaVersion { }

    private class EntityWithoutSchemaVersion { }

    private class DummySync : IEntitySync
    {
        public Task<SyncResult> Run(EntitySyncContext context, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
