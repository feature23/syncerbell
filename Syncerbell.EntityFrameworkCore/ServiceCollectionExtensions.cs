using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Syncerbell.EntityFrameworkCore;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSyncerbellEntityFrameworkCorePersistence(this IServiceCollection services,
        Action<EntityFrameworkCoreSupportOptions>? configureOptions = null)
    {
        var options = new EntityFrameworkCoreSupportOptions();
        configureOptions?.Invoke(options);

        services.AddSingleton(options);

        services.AddDbContext<SyncLogDbContext>(options.ConfigureDbContextOptions);
        services.AddDbContextFactory<SyncLogDbContext>(options.ConfigureDbContextOptions);

        services.AddTransient<ISyncLogPersistence, EntityFrameworkCoreSyncLogPersistence>();

        return services;
    }
}
