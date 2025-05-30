namespace Syncerbell;

/// <summary>
/// A sync eligibility strategy that always returns true, indicating that the entity is always eligible for synchronization.
/// <para />
/// Note that this strategy may be too aggressive in some scenarios, such as when the check interval is very short.
/// </summary>
public class AlwaysEligibleStrategy : ISyncEligibilityStrategy
{
    public ValueTask<bool> IsEligibleToSync(SyncTrigger trigger, SyncEntityOptions entityOptions,
        CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(true);
    }
}
