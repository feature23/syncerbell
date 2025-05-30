namespace Syncerbell.Tests;

public class SyncEntityOptionsTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        const string entityType = "TestEntity";
        var entitySyncType = typeof(TestEntitySync);

        // Act
        var options = new SyncEntityOptions(entityType, entitySyncType);

        // Assert
        Assert.Equal(entityType, options.Entity);
        Assert.Equal(entitySyncType, options.EntitySyncType);

        var strategy = Assert.IsType<IntervalEligibilityStrategy>(options.Eligibility);
        Assert.Equal(TimeSpan.FromDays(1), strategy.Interval);
    }

    private class TestEntitySync : IEntitySync
    {
        public Task<SyncResult> Run(SyncTrigger trigger, SyncEntityOptions entityOptions, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
