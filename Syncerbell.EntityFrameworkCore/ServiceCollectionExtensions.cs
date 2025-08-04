using Microsoft.Extensions.DependencyInjection;

namespace Syncerbell.EntityFrameworkCore;

/// <summary>
/// Provides extension methods for registering Entity Framework Core-based persistence for Syncerbell.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Entity Framework Core-based persistence for sync logs to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the persistence to.</param>
    /// <param name="configureOptions">An optional action to configure <see cref="EntityFrameworkCoreSupportOptions"/>.</param>
    /// <returns>The updated service collection with Entity Framework Core persistence registered.</returns>
    public static IServiceCollection AddSyncerbellEntityFrameworkCorePersistence(this IServiceCollection services,
        Action<EntityFrameworkCoreSupportOptions>? configureOptions = null)
    {
        var options = new EntityFrameworkCoreSupportOptions();
        configureOptions?.Invoke(options);

        services.AddSingleton(options);

        services.AddDbContext<SyncLogDbContext>(options.ConfigureDbContextOptions);

        services.AddTransient<ISyncLogPersistence, EntityFrameworkCoreSyncLogPersistence>();

        return services;
    }
}
