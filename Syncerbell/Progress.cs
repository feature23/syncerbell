namespace Syncerbell;

/// <summary>
/// Represents progress information for a synchronization operation.
/// </summary>
/// <param name="Value">The current progress value.</param>
/// <param name="Max">The maximum progress value.</param>
public record Progress(int Value, int Max);
