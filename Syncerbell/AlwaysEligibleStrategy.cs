namespace Syncerbell;

/// <summary>
/// A sync eligibility strategy that always returns true, indicating that the entity is always eligible for synchronization.
/// <para />
/// Note that this strategy may be too aggressive in some scenarios, such as when the check interval is very short.
/// </summary>
public class AlwaysEligibleStrategy : ISyncEligibilityStrategy
{
    /// <summary>
    /// Determines whether the entity is eligible for synchronization.
    /// </summary>
    /// <param name="trigger">The trigger that initiated the sync check.</param>
    /// <param name="entityOptions">The options for the entity being checked.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="ValueTask{Boolean}"/> that always returns <c>true</c>, indicating the entity is always eligible for synchronization.</returns>
    public ValueTask<bool> IsEligibleToSync(SyncTrigger trigger, SyncEntityOptions entityOptions,
        CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(true);
    }
}
