namespace Syncerbell;

/// <summary>
/// Represents options for synchronizing a specific entity type.
/// </summary>
/// <remarks>
/// This constructor is a lower-level API for creating synchronization options for an entity.
/// For most use cases, you should use the <see cref="Create{T,TSync}(Action{SyncEntityOptions})"/> factory method.
/// </remarks>
/// <param name="entity">The name of the entity to be synchronized.</param>
/// <param name="entitySyncType">The type that implements the <see cref="IEntitySync"/> interface for this entity.</param>
/// <seealso cref="Create{T,TSync}(Action{SyncEntityOptions})"/>
/// <seealso cref="Create{T,TSync}(string, Action{SyncEntityOptions})"/>
public class SyncEntityOptions(string entity, Type entitySyncType)
{
    /// <summary>
    /// The name of the entity to be synchronized.
    /// </summary>
    public string Entity => entity;

    /// <summary>
    /// The optional type of the entity being synchronized.
    /// </summary>
    /// <remarks>
    /// This is useful if the entity type is a .NET type in your application,
    /// but can be null if the entity is a more abstract concept or if the type is not known at compile time.
    /// </remarks>
    public Type? EntityType { get; set; }

    /// <summary>
    /// An optional version of the schema for the entity.
    /// </summary>
    /// <remarks>
    /// This value can be used to track changes in the entity schema over time.
    /// If it differs from the existing sync log entries, it may trigger a full sync depending on
    /// the implementation of the sync logic.
    /// <para />
    /// This value is automatically set by <see cref="Create{T,TSync}(string, Action{SyncEntityOptions})"/>
    /// if the entity type has a <see cref="SchemaVersionAttribute"/> applied to it.
    /// </remarks>
    public int? SchemaVersion { get; set; }

    /// <summary>
    /// The type of the entity sync implementation.
    /// </summary>
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

    /// <summary>
    /// The optional lease expiration time for the sync operation.
    /// If not provided, it will use the global default lease expiration time defined in <see cref="SyncerbellOptions.DefaultLeaseExpiration"/>.
    /// </summary>
    public TimeSpan? LeaseExpiration { get; set; }

    /// <summary>
    /// Creates a new instance of <see cref="SyncEntityOptions"/> for the specified entity type and sync implementation.
    /// </summary>
    /// <param name="configureOptions">A callback to configure additional options for the entity.</param>
    /// <typeparam name="T">The type of the entity to synchronize.</typeparam>
    /// <typeparam name="TSync">The type that implements the <see cref="IEntitySync"/> interface for this entity.</typeparam>
    /// <returns>Returns a new instance of <see cref="SyncEntityOptions"/>.</returns>
    public static SyncEntityOptions Create<T, TSync>(
        Action<SyncEntityOptions>? configureOptions = null)
        where TSync : IEntitySync
        => Create<T, TSync>(typeof(T).Name, configureOptions);

    /// <summary>
    /// Creates a new instance of <see cref="SyncEntityOptions"/> for the specified entity type and sync implementation.
    /// </summary>
    /// <param name="entityName">The name of the entity to synchronize. This overrides the default name which would be derived from the type name.</param>
    /// <param name="configureOptions">A callback to configure additional options for the entity.</param>
    /// <typeparam name="T">The type of the entity to synchronize.</typeparam>
    /// <typeparam name="TSync">The type that implements the <see cref="IEntitySync"/> interface for this entity.</typeparam>
    /// <returns>Returns a new instance of <see cref="SyncEntityOptions"/>.</returns>
    public static SyncEntityOptions Create<T, TSync>(
        string entityName,
        Action<SyncEntityOptions>? configureOptions = null)
        where TSync : IEntitySync
    {
        var options = new SyncEntityOptions(entityName, typeof(TSync))
        {
            EntityType = typeof(T),
        };

        if (typeof(T).GetCustomAttributes(typeof(SchemaVersionAttribute), false).FirstOrDefault() is SchemaVersionAttribute schemaVersionAttribute)
        {
            options.SchemaVersion = schemaVersionAttribute.Version;
        }

        configureOptions?.Invoke(options);
        return options;
    }
}
