namespace Syncerbell.Tests;

public class ProgressPercentageTests
{
    private class TestSyncLogEntry : ISyncLogEntry
    {
        private readonly Guid _id = Guid.NewGuid();

        public int? SchemaVersion => null;
        public SyncTriggerType TriggerType => SyncTriggerType.Manual;
        public SyncStatus SyncStatus { get; set; }
        public DateTime CreatedAt { get; } = DateTime.UtcNow;
        public DateTime? LeasedAt { get; set; }
        public DateTime? LeaseExpiresAt { get; set; }
        public string? LeasedBy { get; set; }
        public string? QueueMessageId { get; set; }
        public DateTime? QueuedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public string? ResultMessage { get; set; }
        public string? HighWaterMark { get; set; }
        public int? ProgressValue { get; set; }
        public int? ProgressMax { get; set; }
        public int? RecordCount { get; set; }
        public string Id => _id.ToString();
        public string Entity => "TestEntity";
        public string? ParametersJson => null;
    }

    [Theory]
    [InlineData(null, 100, null)] // ProgressValue is null
    [InlineData(50, null, null)] // ProgressMax is null
    [InlineData(50, 0, null)] // ProgressMax is zero
    [InlineData(50, -10, null)] // ProgressMax is negative
    [InlineData(25, 100, 0.25f)] // Normal percentage calculation
    [InlineData(100, 100, 1.0f)] // Complete progress
    [InlineData(0, 100, 0.0f)] // Zero progress
    [InlineData(150, 100, 1.5f)] // Progress exceeding max
    [InlineData(1, 3, 0.33333334f)] // Fractional result 1
    [InlineData(2, 3, 0.66666669f)] // Fractional result 2
    [InlineData(7, 11, 0.63636363f)] // Fractional result 3
    [InlineData(13, 17, 0.76470588f)] // Fractional result 4
    [InlineData(-25, 100, -0.25f)] // Negative progress value
    public void ProgressPercentage_WithVariousInputs_ReturnsExpectedResult(int? progressValue, int? progressMax, float? expectedPercentage)
    {
        // Arrange
        ISyncLogEntry logEntry = new TestSyncLogEntry
        {
            ProgressValue = progressValue,
            ProgressMax = progressMax
        };

        // Act
        var percentage = logEntry.ProgressPercentage;

        // Assert
        if (expectedPercentage.HasValue)
        {
            Assert.NotNull(percentage);
            Assert.Equal(expectedPercentage.Value, percentage.Value, precision: 6);
        }
        else
        {
            Assert.Null(percentage);
        }
    }
}
