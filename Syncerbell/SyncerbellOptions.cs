namespace Syncerbell;

/// <summary>
/// Options for configuring the Syncerbell service.
/// </summary>
public class SyncerbellOptions
{
    /// <summary>
    /// The collection of entities to synchronize.
    /// </summary>
    /// <remarks>
    /// The <see cref="AddEntity{T,TSync}"/> method is a more convenient way to add entities to this collection
    /// than adding to this collection directly.
    /// </remarks>
    /// <seealso cref="AddEntity{T,TSync}"/>
    /// <seealso cref="AddEntity"/>
    public IList<SyncEntityOptions> Entities { get; set; } = [];

    /// <summary>
    /// A function that provides the machine ID to use for lease management.
    /// The default implementation uses the current machine name.
    /// </summary>
    public Func<string> MachineIdProvider { get; set; } = () => Environment.MachineName;

    /// <summary>
    /// The default duration for a lease on a sync operation, which is used if not specified in the entity options.
    /// The default value is 1 day.
    /// </summary>
    public TimeSpan DefaultLeaseExpiration { get; set; } = TimeSpan.FromDays(1);

    /// <summary>
    /// The type of entity provider to use for determining which entities are eligible for synchronization.
    /// </summary>
    /// <seealso cref="WithEntityProvider{T}"/>
    public Type? EntityProviderType { get; set; }

    /// <summary>
    /// Specifies an entity provider to use for determining which entities are eligible for synchronization.
    /// <para />
    /// This is optional. If not set, the <see cref="ISyncService"/> will only use the entities defined in the
    /// <see cref="Entities"/> collection.
    /// If set, the <see cref="ISyncService"/> will call the provider to get additional entities to sync at runtime, in
    /// addition to the ones defined in the <see cref="Entities"/> collection.
    /// </summary>
    /// <remarks>
    /// This type will be registered as a singleton in the service collection, so it should be thread-safe.
    /// </remarks>
    /// <typeparam name="T">The type of entity provider to register.</typeparam>
    /// <returns>Returns the options instance for method chaining.</returns>
    public SyncerbellOptions WithEntityProvider<T>()
        where T : IEntityProvider
    {
        EntityProviderType = typeof(T);
        return this;
    }

    /// <summary>
    /// Adds an entity to the synchronization options. This overload allows you to specify the entity name and type
    /// explicitly, but <see cref="AddEntity{T,TSync}"/> is a more user-friendly way to add entities if you have
    /// access to the entity type and its sync implementation.
    /// </summary>
    /// <param name="entity">The name of the entity to synchronize.</param>
    /// <param name="entitySyncType">The type that implements the <see cref="IEntitySync"/> interface for this entity.</param>
    /// <param name="configureOptions">A callback to configure additional options for the entity.</param>
    /// <returns>Returns the options instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown if the provided type does not implement the <see cref="IEntitySync"/> interface.</exception>
    /// <seealso cref="AddEntity{T,TSync}"/>
    /// <seealso cref="Entities"/>
    public SyncerbellOptions AddEntity(string entity, Type entitySyncType, Action<SyncEntityOptions>? configureOptions = null)
    {
        if (!entitySyncType.IsAssignableTo(typeof(IEntitySync)))
        {
            throw new ArgumentException($"Type {entitySyncType.Name} must implement the {nameof(IEntitySync)} interface.");
        }

        var options = new SyncEntityOptions(entity, entitySyncType);
        configureOptions?.Invoke(options);
        Entities.Add(options);

        return this;
    }

    /// <summary>
    /// Adds an entity to the synchronization options using a strongly-typed, generic approach.
    /// </summary>
    /// <param name="configureOptions">A callback to configure additional options for the entity.</param>
    /// <typeparam name="T">The type of the entity to synchronize. This type must have a corresponding sync implementation.</typeparam>
    /// <typeparam name="TSync">The type that implements the <see cref="IEntitySync"/> interface for this entity.</typeparam>
    /// <returns>Returns the options instance for method chaining.</returns>
    /// <seealso cref="AddEntity"/>
    /// <seealso cref="Entities"/>
    public SyncerbellOptions AddEntity<T, TSync>(Action<SyncEntityOptions>? configureOptions = null)
        where TSync : IEntitySync
    {
        var options = SyncEntityOptions.Create<T, TSync>(configureOptions);
        Entities.Add(options);

        return this;
    }
}
