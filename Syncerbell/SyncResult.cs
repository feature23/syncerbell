namespace Syncerbell;

public record SyncResult(bool Success, string? Message = null, string? HighWaterMark = null);
