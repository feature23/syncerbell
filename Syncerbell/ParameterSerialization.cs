using System.Text.Json;

namespace Syncerbell;

/// <summary>
/// Provides methods for serializing parameters used in synchronization operations.
/// </summary>
public static class ParameterSerialization
{
    /// <summary>
    /// Serializes a dictionary of parameters into a JSON string.
    /// </summary>
    /// <param name="parameters">The parameters to serialize.</param>
    /// <returns>A JSON string representation of the parameters, or null if the parameters are null or empty.</returns>
    public static string? Serialize(SortedDictionary<string, object?>? parameters)
        => parameters == null || parameters.Count == 0
        ? null
        : JsonSerializer.Serialize(parameters);
}
