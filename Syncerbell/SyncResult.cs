namespace Syncerbell;

public record SyncResult(SyncEntityOptions Entity, bool Success, string? Message = null, string? HighWaterMark = null);
