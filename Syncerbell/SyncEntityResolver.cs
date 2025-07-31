using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Syncerbell;

/// <summary>
/// Resolves sync entities by combining configured entities with additional entities from an optional entity provider.
/// </summary>
public class SyncEntityResolver(
    SyncerbellOptions options,
    IServiceProvider serviceProvider,
    ILogger<SyncEntityResolver> logger)
{
    /// <summary>
    /// Resolves all sync entities by combining configured entities with additional entities from the entity provider.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A read-only list of sync entity options containing all resolved entities.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the entity provider type is not registered or does not implement <see cref="IEntityProvider"/>.</exception>
    public async Task<IReadOnlyList<SyncEntityOptions>> ResolveEntities(CancellationToken cancellationToken = default)
    {
        var entities = new List<SyncEntityOptions>(options.Entities);

        if (options.EntityProviderType is { } entityProviderType)
        {
            var entityProvider = serviceProvider.GetRequiredService(entityProviderType) as IEntityProvider
                                 ?? throw new InvalidOperationException(
                                     $"Entity provider type {entityProviderType.FullName} is not registered or does not implement {nameof(IEntityProvider)}.");

            var additionalEntities = await entityProvider.GetEntities(cancellationToken);

            if (additionalEntities.Count == 0)
            {
                logger.LogWarning("Entity provider returned no additional entities. Using configured entities only.");
            }
            else
            {
                logger.LogInformation("Entity provider returned {Count} additional entities.", additionalEntities.Count);
                entities.AddRange(additionalEntities);
            }
        }

        return entities;
    }
}
