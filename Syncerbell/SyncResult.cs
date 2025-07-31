namespace Syncerbell;

/// <summary>
/// Represents the result of a synchronization operation, including entity, status, message, high-water mark, and record count.
/// </summary>
/// <param name="Entity">The entity options associated with the sync operation.</param>
/// <param name="Success">Indicates whether the sync operation was successful.</param>
/// <param name="Message">An optional message describing the result of the sync operation.</param>
/// <param name="HighWaterMark">An optional high-water mark value for incremental sync scenarios.</param>
/// <param name="RecordCount">An optional count of records processed during the sync operation.</param>
public record SyncResult(SyncEntityOptions Entity, bool Success, string? Message = null, string? HighWaterMark = null, int? RecordCount = null);
