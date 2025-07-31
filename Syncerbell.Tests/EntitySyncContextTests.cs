namespace Syncerbell.Tests;

public class EntitySyncContextTests
{
    [Fact]
    public async Task ReportProgress_WithValidParameters_CallsProgressReporter()
    {
        // Arrange
        var progressReported = false;
        Progress? reportedProgress = null;

        Task ProgressReporter(Progress progress)
        {
            progressReported = true;
            reportedProgress = progress;
            return Task.CompletedTask;
        }

        var trigger = new SyncTrigger
        {
            TriggerType = SyncTriggerType.Manual,
            PriorSyncInfo = new PriorSyncInfo
            {
                HighWaterMark = null,
                LastSyncCreatedAt = null,
                LastSyncLeasedAt = null,
                LastSyncCompletedAt = null
            }
        };
        var entity = new SyncEntityOptions("TestEntity", typeof(object));
        var context = new EntitySyncContext(trigger, entity, ProgressReporter);

        // Act
        await context.ReportProgress(25, 100);

        // Assert
        Assert.True(progressReported);
        Assert.NotNull(reportedProgress);
        Assert.Equal(25, reportedProgress.Value.Value);
        Assert.Equal(100, reportedProgress.Value.Max);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(50, 100)]
    [InlineData(100, 100)]
    [InlineData(1, 10)]
    public async Task ReportProgress_WithVariousValidInputs_ReportsCorrectProgress(int value, int max)
    {
        // Arrange
        Progress? reportedProgress = null;

        Task ProgressReporter(Progress progress)
        {
            reportedProgress = progress;
            return Task.CompletedTask;
        }

        var trigger = new SyncTrigger
        {
            TriggerType = SyncTriggerType.Manual,
            PriorSyncInfo = new PriorSyncInfo
            {
                HighWaterMark = null,
                LastSyncCreatedAt = null,
                LastSyncLeasedAt = null,
                LastSyncCompletedAt = null
            }
        };
        var entity = new SyncEntityOptions("TestEntity", typeof(object));
        var context = new EntitySyncContext(trigger, entity, ProgressReporter);

        // Act
        await context.ReportProgress(value, max);

        // Assert
        Assert.NotNull(reportedProgress);
        Assert.Equal(value, reportedProgress.Value.Value);
        Assert.Equal(max, reportedProgress.Value.Max);
    }

    [Theory]
    [InlineData(-1, 10)]
    [InlineData(-5, 100)]
    public async Task ReportProgress_WithNegativeValue_ThrowsArgumentOutOfRangeException(int value, int max)
    {
        // Arrange
        var trigger = new SyncTrigger
        {
            TriggerType = SyncTriggerType.Manual,
            PriorSyncInfo = new PriorSyncInfo
            {
                HighWaterMark = null,
                LastSyncCreatedAt = null,
                LastSyncLeasedAt = null,
                LastSyncCompletedAt = null
            }
        };
        var entity = new SyncEntityOptions("TestEntity", typeof(object));
        var context = new EntitySyncContext(trigger, entity, _ => Task.CompletedTask);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => context.ReportProgress(value, max));
        Assert.Equal("Value", exception.ParamName);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(5, -1)]
    public async Task ReportProgress_WithNonPositiveMax_ThrowsArgumentOutOfRangeException(int value, int max)
    {
        // Arrange
        var trigger = new SyncTrigger
        {
            TriggerType = SyncTriggerType.Manual,
            PriorSyncInfo = new PriorSyncInfo
            {
                HighWaterMark = null,
                LastSyncCreatedAt = null,
                LastSyncLeasedAt = null,
                LastSyncCompletedAt = null
            }
        };
        var entity = new SyncEntityOptions("TestEntity", typeof(object));
        var context = new EntitySyncContext(trigger, entity, _ => Task.CompletedTask);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => context.ReportProgress(value, max));
        Assert.Equal("Max", exception.ParamName);
    }

    [Theory]
    [InlineData(10, 5)]
    [InlineData(100, 50)]
    public async Task ReportProgress_WithValueGreaterThanMax_ThrowsArgumentOutOfRangeException(int value, int max)
    {
        // Arrange
        var trigger = new SyncTrigger
        {
            TriggerType = SyncTriggerType.Manual,
            PriorSyncInfo = new PriorSyncInfo
            {
                HighWaterMark = null,
                LastSyncCreatedAt = null,
                LastSyncLeasedAt = null,
                LastSyncCompletedAt = null
            }
        };
        var entity = new SyncEntityOptions("TestEntity", typeof(object));
        var context = new EntitySyncContext(trigger, entity, _ => Task.CompletedTask);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => context.ReportProgress(value, max));
        Assert.Equal("Value", exception.ParamName);
    }

    [Fact]
    public async Task ReportProgress_WithAsyncProgressReporter_AwaitsCompletion()
    {
        // Arrange
        var taskCompleted = false;

        async Task ProgressReporter(Progress progress)
        {
            await Task.Delay(10); // Simulate async work
            taskCompleted = true;
        }

        var trigger = new SyncTrigger
        {
            TriggerType = SyncTriggerType.Manual,
            PriorSyncInfo = new PriorSyncInfo
            {
                HighWaterMark = null,
                LastSyncCreatedAt = null,
                LastSyncLeasedAt = null,
                LastSyncCompletedAt = null
            }
        };
        var entity = new SyncEntityOptions("TestEntity", typeof(object));
        var context = new EntitySyncContext(trigger, entity, ProgressReporter);

        // Act
        await context.ReportProgress(50, 100);

        // Assert
        Assert.True(taskCompleted);
    }

    [Fact]
    public async Task ReportProgress_WhenProgressReporterThrows_PropagatesException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test exception");

        Task ProgressReporter(Progress progress)
        {
            throw expectedException;
        }

        var trigger = new SyncTrigger
        {
            TriggerType = SyncTriggerType.Manual,
            PriorSyncInfo = new PriorSyncInfo
            {
                HighWaterMark = null,
                LastSyncCreatedAt = null,
                LastSyncLeasedAt = null,
                LastSyncCompletedAt = null
            }
        };
        var entity = new SyncEntityOptions("TestEntity", typeof(object));
        var context = new EntitySyncContext(trigger, entity, ProgressReporter);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(() => context.ReportProgress(50, 100));
        Assert.Same(expectedException, thrownException);
    }
}
