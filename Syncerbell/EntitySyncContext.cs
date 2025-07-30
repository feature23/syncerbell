namespace Syncerbell;

/// <summary>
/// Represents the context for an entity synchronization operation.
/// </summary>
/// <param name="Trigger">The trigger that initiated the sync operation.</param>
/// <param name="Entity">The configured options for the entity to be synchronized.</param>
/// <param name="ReportProgress">A function to report progress during the sync operation.</param>
public record EntitySyncContext(SyncTrigger Trigger, SyncEntityOptions Entity, Func<Progress, Task> ReportProgress);
