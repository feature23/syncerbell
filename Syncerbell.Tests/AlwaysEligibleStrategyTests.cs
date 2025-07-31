namespace Syncerbell.Tests;

public class AlwaysEligibleStrategyTests
{
    [InlineData(SyncTriggerType.Manual, null)]
    [InlineData(SyncTriggerType.Manual, 30)]
    [InlineData(SyncTriggerType.Timer, null)]
    [InlineData(SyncTriggerType.Timer, 400)]
    [InlineData(SyncTriggerType.Unknown, null)]
    [InlineData(SyncTriggerType.Unknown, -1)] // in the future? doesn't matter.
    [Theory]
    public async Task AlwaysEligible_AlwaysReturnsTrue(SyncTriggerType syncTriggerType, int? lastSyncDaysAgo)
    {
        // Arrange
        var strategy = new AlwaysEligibleStrategy();

        // Act
        var isEligibleToSync = await strategy.IsEligibleToSync(new SyncTrigger
        {
            PriorSyncInfo = new PriorSyncInfo()
            {
                LastSyncLeasedAt = lastSyncDaysAgo.HasValue ? DateTime.UtcNow.AddDays(-1 * lastSyncDaysAgo.Value) : null,
                LastSyncCreatedAt = lastSyncDaysAgo.HasValue ? DateTime.UtcNow.AddDays(-1 * lastSyncDaysAgo.Value) : null,
                LastSyncCompletedAt = null,
                HighWaterMark = null,
            },
            TriggerType = syncTriggerType,
        }, new SyncEntityOptions("TestEntity", typeof(TestEntitySync)));

        // Assert
        Assert.True(isEligibleToSync);
    }

    private class TestEntitySync : IEntitySync
    {
        public Task<SyncResult> Run(EntitySyncContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new SyncResult(context.Entity, true));
        }
    }
}
