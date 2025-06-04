namespace Syncerbell;

/// <summary>
/// A provider interface for retrieving which entities should be synchronized.
/// <para />
/// This allows you to dynamically determine the entities to sync at runtime, instead of hardcoding them in
/// the <see cref="SyncerbellOptions"/> at startup. This will be invoked by the <see cref="ISyncService"/>
/// each time a sync operation is triggered. This is especially useful for multi-tenant applications
/// or any scenario where the entities have dynamic parameters.
/// </summary>
public interface IEntityProvider
{
    /// <summary>
    /// Gets the list of entities to synchronize.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Returns a list of entities to synchronize. This result must not be null, but may be empty.</returns>
    Task<IReadOnlyList<SyncEntityOptions>> GetEntities(CancellationToken cancellationToken = default);
}
