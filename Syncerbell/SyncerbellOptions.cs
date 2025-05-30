namespace Syncerbell;

public class SyncerbellOptions
{
    public IList<SyncEntityOptions> Entities { get; set; } = [];

    public Func<string> MachineIdProvider { get; set; } = () => Environment.MachineName;

    public TimeSpan DefaultLeaseExpiration { get; set; } = TimeSpan.FromDays(1);

    public void AddEntity(string entity, Type entitySyncType, Action<SyncEntityOptions>? configureOptions = null)
    {
        if (!entitySyncType.IsAssignableTo(typeof(IEntitySync)))
        {
            throw new ArgumentException($"Type {entitySyncType.Name} must implement the {nameof(IEntitySync)} interface.");
        }

        var options = new SyncEntityOptions(entity, entitySyncType);
        configureOptions?.Invoke(options);
        Entities.Add(options);
    }

    public void AddEntity<T, TSync>(Action<SyncEntityOptions>? configureOptions = null)
        where TSync : IEntitySync
        => AddEntity(typeof(T).Name, typeof(TSync), configureOptions);
}
