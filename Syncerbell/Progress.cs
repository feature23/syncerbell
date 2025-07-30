namespace Syncerbell;

/// <summary>
/// Represents progress information for a synchronization operation.
/// </summary>
public record struct Progress
{
    /// <summary>
    /// Represents progress information for a synchronization operation.
    /// </summary>
    /// <param name="Value">The current progress value.</param>
    /// <param name="Max">The maximum progress value.</param>
    public Progress(int Value, int Max)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(Value, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(Max, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(Value, Max);

        this.Value = Value;
        this.Max = Max;
    }

    /// <summary>The current progress value.</summary>
    public int Value { get; }

    /// <summary>The maximum progress value.</summary>
    public int Max { get; }
}
