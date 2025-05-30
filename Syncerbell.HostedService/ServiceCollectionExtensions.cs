using Microsoft.Extensions.DependencyInjection;

namespace Syncerbell.HostedService;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSyncerbellHostedService(
        this IServiceCollection services,
        Action<SyncerbellHostedServiceOptions>? configureOptions = null)
    {
        var options = new SyncerbellHostedServiceOptions();
        configureOptions?.Invoke(options);

        services.AddSingleton(options);
        services.AddHostedService<SyncerbellHostedService>();

        return services;
    }
}
