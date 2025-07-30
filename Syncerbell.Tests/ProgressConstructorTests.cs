namespace Syncerbell.Tests;

public class ProgressConstructorTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    [InlineData(5, 10)]
    [InlineData(100, 100)]
    public void Constructor_WithValidParameters_CreatesProgressSuccessfully(int value, int max)
    {
        // Act
        var progress = new Progress(value, max);

        // Assert
        Assert.Equal(value, progress.Value);
        Assert.Equal(max, progress.Max);
    }

    [Theory]
    [InlineData(-1, 10)]
    [InlineData(-5, 100)]
    [InlineData(int.MinValue, 1)]
    public void Constructor_WithNegativeValue_ThrowsArgumentOutOfRangeException(int value, int max)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Progress(value, max));
        Assert.Equal("Value", exception.ParamName);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(5, -1)]
    [InlineData(10, -10)]
    [InlineData(0, int.MinValue)]
    public void Constructor_WithNonPositiveMax_ThrowsArgumentOutOfRangeException(int value, int max)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Progress(value, max));
        Assert.Equal("Max", exception.ParamName);
    }

    [Theory]
    [InlineData(2, 1)]
    [InlineData(10, 5)]
    [InlineData(100, 50)]
    [InlineData(int.MaxValue, 1)]
    public void Constructor_WithValueGreaterThanMax_ThrowsArgumentOutOfRangeException(int value, int max)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Progress(value, max));
        Assert.Equal("Value", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithMinValues_CreatesProgressSuccessfully()
    {
        // Act
        var progress = new Progress(0, 1);

        // Assert
        Assert.Equal(0, progress.Value);
        Assert.Equal(1, progress.Max);
    }

    [Fact]
    public void Constructor_WithMaxIntValues_CreatesProgressSuccessfully()
    {
        // Act
        var progress = new Progress(int.MaxValue, int.MaxValue);

        // Assert
        Assert.Equal(int.MaxValue, progress.Value);
        Assert.Equal(int.MaxValue, progress.Max);
    }
}
