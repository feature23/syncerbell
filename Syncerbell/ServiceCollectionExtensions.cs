using Microsoft.Extensions.DependencyInjection;

namespace Syncerbell;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSyncerbell(this IServiceCollection services, Action<SyncerbellOptions>? configureOptions)
    {
        var options = new SyncerbellOptions();
        configureOptions?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<ISyncService, SyncService>();

        foreach (var entity in options.Entities)
        {
            services.AddTransient(entity.EntitySyncType);
        }

        if (options.EntityProviderType != null)
        {
            services.AddSingleton(options.EntityProviderType);
        }

        return services;
    }

    /// <summary>
    /// Adds in-memory persistence for sync logs.
    /// <para />
    /// This is useful for testing or simple applications where you don't need a persistent store.
    /// </summary>
    /// <param name="services">The service collection to add the persistence to.</param>
    /// <returns>The updated service collection with in-memory persistence registered.</returns>
    public static IServiceCollection AddSyncerbellInMemoryPersistence(this IServiceCollection services)
    {
        services.AddSingleton<ISyncLogPersistence, InMemorySyncLogPersistence>();
        return services;
    }
}
