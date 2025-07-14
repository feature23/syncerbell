using Microsoft.Extensions.DependencyInjection;

namespace Syncerbell.HostedService;

/// <summary>
/// Provides extension methods for registering the Syncerbell hosted service with an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Syncerbell hosted service to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the hosted service to.</param>
    /// <param name="configureOptions">An optional action to configure <see cref="SyncerbellHostedServiceOptions"/>.</param>
    /// <returns>The updated service collection with the Syncerbell hosted service registered.</returns>
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
