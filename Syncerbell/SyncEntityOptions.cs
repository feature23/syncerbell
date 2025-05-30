namespace Syncerbell;

public class SyncEntityOptions(string entity, Type entitySyncType)
{
    public string Entity => entity;

    public Type EntitySyncType => entitySyncType;

    /// <summary>
    /// A dictionary of parameters to distinguish different sync operations for the same entity.
    /// <para />
    /// For example, this could be a tenant ID, user ID, or any other identifier that helps
    /// differentiate sync operations for the same entity type.
    /// <para />
    /// Ensure that the keys and values are stable and consistent across sync operations,
    /// otherwise the sync operation may not work as expected (such as always starting from the beginning of time).
    /// <para />
    /// Additionally, the values should be serializable to JSON, as they may be stored in a database or sent over the network.
    /// If using a complex type for any values, ensure it is serializable and has a stable JSON representation.
    /// </summary>
    /// <remarks>
    /// This is implemented as a sorted dictionary to ensure that the parameters are always in a consistent order.
    /// </remarks>
    public SortedDictionary<string, object?>? Parameters { get; set; }

    /// <summary>
    /// Gets or sets the strategy to determine if the entity is eligible for synchronization.
    /// <para />
    /// Default is an <see cref="IntervalEligibilityStrategy"/> with a 1-day interval.
    /// </summary>
    public ISyncEligibilityStrategy Eligibility { get; set; } = new IntervalEligibilityStrategy(TimeSpan.FromDays(1));

    public TimeSpan? LeaseExpiration { get; set; }
}
