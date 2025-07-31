using System.Text.Json;

namespace Syncerbell.Tests;

public class ParameterSerializationTests
{
    [Fact]
    public void Serialize_WithNullParameters_ReturnsNull()
    {
        // Arrange
        SortedDictionary<string, object?>? parameters = null;

        // Act
        var result = ParameterSerialization.Serialize(parameters);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Serialize_WithEmptyDictionary_ReturnsNull()
    {
        // Arrange
        var parameters = new SortedDictionary<string, object?>();

        // Act
        var result = ParameterSerialization.Serialize(parameters);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Serialize_WithSingleStringParameter_ReturnsValidJson()
    {
        // Arrange
        var parameters = new SortedDictionary<string, object?>
        {
            { "key1", "value1" }
        };

        // Act
        var result = ParameterSerialization.Serialize(parameters);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("{\"key1\":\"value1\"}", result);
    }

    [Fact]
    public void Serialize_WithMultipleParameters_ReturnsValidJson()
    {
        // Arrange
        var parameters = new SortedDictionary<string, object?>
        {
            { "stringParam", "test" },
            { "intParam", 42 },
            { "boolParam", true }
        };

        // Act
        var result = ParameterSerialization.Serialize(parameters);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("{\"boolParam\":true,\"intParam\":42,\"stringParam\":\"test\"}", result);
    }

    [Fact]
    public void Serialize_WithNullValues_ReturnsValidJson()
    {
        // Arrange
        var parameters = new SortedDictionary<string, object?>
        {
            { "nullParam", null },
            { "stringParam", "test" }
        };

        // Act
        var result = ParameterSerialization.Serialize(parameters);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("{\"nullParam\":null,\"stringParam\":\"test\"}", result);
    }

    [Fact]
    public void Serialize_WithComplexObjects_ReturnsValidJson()
    {
        // Arrange
        var complexObject = new { Name = "Test", Value = 123 };
        var parameters = new SortedDictionary<string, object?>
        {
            { "complex", complexObject },
            { "array", new[] { 1, 2, 3 } }
        };

        // Act
        var result = ParameterSerialization.Serialize(parameters);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("{\"array\":[1,2,3],\"complex\":{\"Name\":\"Test\",\"Value\":123}}", result);
    }

    [Fact]
    public void Serialize_PreservesKeyOrder_WhenUsingSortedDictionary()
    {
        // Arrange
        var parameters = new SortedDictionary<string, object?>
        {
            { "zKey", "last" },
            { "aKey", "first" },
            { "mKey", "middle" }
        };

        // Act
        var result = ParameterSerialization.Serialize(parameters);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("{\"aKey\":\"first\",\"mKey\":\"middle\",\"zKey\":\"last\"}", result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("special-chars!@#$%")]
    public void Serialize_WithVariousStringValues_ReturnsValidJson(string value)
    {
        // Arrange
        var parameters = new SortedDictionary<string, object?>
        {
            { "testKey", value }
        };

        // Act
        var result = ParameterSerialization.Serialize(parameters);

        // Assert
        Assert.NotNull(result);
        var deserialized = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(result);
        Assert.Equal(value, deserialized!["testKey"].GetString());
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(0)]
    [InlineData(int.MaxValue)]
    public void Serialize_WithVariousIntegerValues_ReturnsValidJson(int value)
    {
        // Arrange
        var parameters = new SortedDictionary<string, object?>
        {
            { "intKey", value }
        };

        // Act
        var result = ParameterSerialization.Serialize(parameters);

        // Assert
        Assert.NotNull(result);
        var deserialized = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(result);
        Assert.Equal(value, deserialized!["intKey"].GetInt32());
    }
}
