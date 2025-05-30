namespace Syncerbell.Tests;

public class IntervalEligibilityStrategyTests
{
    [InlineData(1, null, true)]
    [InlineData(7, 1, false)]
    [InlineData(7, 7, true)]
    [InlineData(30, 15, false)]
    [InlineData(30, 300, true)]
    [Theory]
    public async Task IntervalEligibility_AlwaysReturnsTrue(int intervalDays, int? lastSyncDaysAgo, bool shouldBeEligible)
    {
        // Arrange
        var strategy = new IntervalEligibilityStrategy(TimeSpan.FromDays(intervalDays));

        // Act
        var isEligibleToSync = await strategy.IsEligibleToSync(new SyncTrigger
        {
            PriorSyncInfo = new PriorSyncInfo()
            {
                LastSyncLeasedAt = lastSyncDaysAgo.HasValue ? DateTime.UtcNow.AddDays(-1 * lastSyncDaysAgo.Value) : null,
                LastSyncQueuedAt = lastSyncDaysAgo.HasValue ? DateTime.UtcNow.AddDays(-1 * lastSyncDaysAgo.Value) : null,
                LastSyncCompletedAt = null,
                HighWaterMark = null,
            },
            TriggerType = SyncTriggerType.Timer,
        }, new SyncEntityOptions("TestEntity", typeof(TestEntitySync)));

        // Assert
        Assert.Equal(shouldBeEligible, isEligibleToSync);
    }

    private class TestEntitySync : IEntitySync
    {
        public Task<SyncResult> Run(SyncTrigger trigger,SyncEntityOptions entityOptions, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
