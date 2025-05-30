namespace Syncerbell;

public interface ISyncService
{
    Task SyncAllIfEligible(
        SyncTriggerType triggerType,
        CancellationToken cancellationToken = default);
}
