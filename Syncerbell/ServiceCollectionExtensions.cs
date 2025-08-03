using Microsoft.Extensions.DependencyInjection;

namespace Syncerbell;

/// <summary>
/// Provides extension methods for registering Syncerbell services and persistence with an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Syncerbell core services and configuration to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add Syncerbell to.</param>
    /// <param name="configureOptions">An action to configure <see cref="SyncerbellOptions"/>.</param>
    /// <returns>The updated service collection with Syncerbell services registered.</returns>
    public static IServiceCollection AddSyncerbell(this IServiceCollection services, Action<SyncerbellOptions>? configureOptions)
    {
        var options = new SyncerbellOptions();
        configureOptions?.Invoke(options);

        services.AddSingleton(options);
        services.AddTransient<ISyncService, SyncService>();
        services.AddTransient<ISyncQueueService, SyncQueueService>();
        services.AddTransient<SyncEntityResolver>();

        foreach (var entity in options.Entities)
        {
            services.AddTransient(entity.EntitySyncType);
        }

        if (options.EntityProviderType != null)
        {
            services.AddTransient(options.EntityProviderType);
        }

        return services;
    }

    /// <summary>
    /// Adds in-memory persistence for sync logs.
    /// This is useful for testing or simple applications where you don't need a persistent store.
    /// </summary>
    /// <param name="services">The service collection to add the persistence to.</param>
    /// <returns>The updated service collection with in-memory persistence registered.</returns>
    public static IServiceCollection AddSyncerbellInMemoryPersistence(this IServiceCollection services)
    {
        services.AddTransient<ISyncLogPersistence, InMemorySyncLogPersistence>();
        return services;
    }
}
