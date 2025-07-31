namespace Syncerbell;

/// <summary>
/// Represents the behavior for acquiring a lease of a sync log entry during synchronization operations.
/// </summary>
public enum AcquireLeaseBehavior
{
    /// <summary>
    /// Do not acquire a lease for the sync log entry.
    /// If the entry is unleased or created, this will leave the entry unleased,
    /// allowing other processes to acquire it if needed.
    /// If the entry is already leased, it will fail to acquire the lease.
    /// </summary>
    DoNotAcquire = 0,

    /// <summary>
    /// Try to acquire a lease for the sync log entry only if it is not already leased.
    /// If the entry is already leased, it will fail to acquire the lease.
    /// </summary>
    AcquireIfNotLeased = 1,

    /// <summary>
    /// Acquire a lease for the sync log entry, regardless of its current leased state.
    /// This will override any existing lease, effectively leasing the entry to the current process.
    /// </summary>
    ForceAcquire = 2,
}
