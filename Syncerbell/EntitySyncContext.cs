namespace Syncerbell;

/// <summary>
/// Represents the context for an entity synchronization operation.
/// </summary>
/// <param name="Trigger">The trigger that initiated the sync operation.</param>
/// <param name="Entity">The configured options for the entity to be synchronized.</param>
/// <param name="ProgressReporter">A function to report progress during the sync operation.</param>
public record EntitySyncContext(SyncTrigger Trigger, SyncEntityOptions Entity, Func<Progress, Task> ProgressReporter)
{
    /// <summary>
    /// Reports progress by creating a Progress object and calling the ProgressReporter.
    /// </summary>
    /// <param name="value">The current progress value.</param>
    /// <param name="max">The maximum progress value.</param>
    /// <returns>A task representing the asynchronous progress reporting operation.</returns>
    public Task ReportProgress(int value, int max) => ProgressReporter(new Progress(value, max));
}
