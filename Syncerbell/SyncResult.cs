namespace Syncerbell;

/// <summary>
/// Represents the result of a synchronization operation, including entity, status, message, and high-water mark.
/// </summary>
/// <param name="Entity">The entity options associated with the sync operation.</param>
/// <param name="Success">Indicates whether the sync operation was successful.</param>
/// <param name="Message">An optional message describing the result of the sync operation.</param>
/// <param name="HighWaterMark">An optional high-water mark value for incremental sync scenarios.</param>
public record SyncResult(SyncEntityOptions Entity, bool Success, string? Message = null, string? HighWaterMark = null);
