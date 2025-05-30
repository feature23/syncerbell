namespace Syncerbell;

public class EntitySyncResolver(IServiceProvider serviceProvider)
{
    public IEntitySync Resolve(SyncEntityOptions options)
    {
        // TODO: support keyed services
        return serviceProvider.GetService(options.EntitySyncType) as IEntitySync
            ?? throw new InvalidOperationException($"No {nameof(IEntitySync)} service registered for entity {options.EntitySyncType.FullName}");
    }
}
