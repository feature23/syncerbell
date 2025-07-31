namespace Syncerbell;

/// <summary>
/// Contains information about the previous synchronization state for an entity, including high-water mark and sync timestamps.
/// </summary>
public class PriorSyncInfo
{
    /// <summary>
    /// Gets the high-water mark value for incremental sync scenarios.
    /// </summary>
    public required string? HighWaterMark { get; init; }

    /// <summary>
    /// Gets or sets the date and time when the last sync log entry was created for the entity.
    /// </summary>
    public required DateTime? LastSyncCreatedAt { get; init; }

    /// <summary>
    /// Gets or sets the date and time when the last sync was leased for the entity.
    /// </summary>
    public required DateTime? LastSyncLeasedAt { get; init; }

    /// <summary>
    /// Gets or sets the date and time when the last sync was completed for the entity.
    /// </summary>
    public required DateTime? LastSyncCompletedAt { get; init; }
}
