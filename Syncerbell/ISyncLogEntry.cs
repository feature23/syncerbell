namespace Syncerbell;

/// <summary>
/// Represents a log entry for a synchronization operation, including status, timing, and result information.
/// </summary>
public interface ISyncLogEntry
{
    /// <summary>
    /// Gets an identifier for locating the sync operation in logs or other systems.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the name of the entity being synchronized.
    /// </summary>
    string Entity { get; }

    /// <summary>
    /// Gets the serialized parameters associated with the entity, if any.
    /// </summary>
    string? ParametersJson { get; }

    /// <summary>
    /// Gets the schema version of the entity, if specified.
    /// </summary>
    int? SchemaVersion { get; }

    /// <summary>
    /// Gets the type of trigger that initiated the sync operation.
    /// </summary>
    SyncTriggerType TriggerType { get; }

    /// <summary>
    /// Gets or sets the current status of the sync operation.
    /// </summary>
    SyncStatus SyncStatus { get; set; }

    /// <summary>
    /// Gets the date and time when the log entry was created.
    /// </summary>
    DateTime CreatedAt { get; }

    /// <summary>
    /// Gets or sets the date and time when the sync operation was leased.
    /// </summary>
    DateTime? LeasedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the lease for the sync operation expires.
    /// </summary>
    DateTime? LeaseExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the machine or process that leased the sync operation.
    /// </summary>
    string? LeasedBy { get; set; }

    /// <summary>
    /// Gets or sets the message ID when fanning out sync operations via a queue.
    /// </summary>
    string? QueueMessageId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the sync operation was queued.
    /// </summary>
    DateTime? QueuedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the sync operation finished.
    /// </summary>
    DateTime? FinishedAt { get; set; }

    /// <summary>
    /// Gets or sets the result message of the sync operation, if any.
    /// </summary>
    string? ResultMessage { get; set; }

    /// <summary>
    /// Gets or sets the high-water mark value for incremental sync scenarios.
    /// </summary>
    string? HighWaterMark { get; set; }

    /// <summary>
    /// Gets or sets the latest progress amount for the sync operation.
    /// Can be null if progress tracking is not implemented or if the operation has not yet started.
    /// </summary>
    int? ProgressValue { get; set; }

    /// <summary>
    /// Gets or sets the maximum value for progress tracking.
    /// This is useful for operations where progress can be measured against a known maximum,
    /// such as the total number of items to be processed.
    /// Can be null if the maximum is not known or applicable.
    /// </summary>
    int? ProgressMax { get; set; }

    /// <summary>
    /// Gets the progress percentage as a float value between 0 and 1.
    /// Multiply by 100 to get a percentage value for display purposes.
    /// Returns null if ProgressValue is not set or ProgressMax is zero or less.
    /// </summary>
    float? ProgressPercentage => ProgressValue.HasValue && ProgressMax is > 0
        ? (float)ProgressValue.Value / ProgressMax.Value
        : null;

    /// <summary>
    /// Gets or sets the total number of records processed during the sync operation.
    /// </summary>
    int? RecordCount { get; set; }
}
