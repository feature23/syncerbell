using Microsoft.Extensions.DependencyInjection;

namespace Syncerbell.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddSyncerbell_WithEmptyOptions_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(); // Required for ISyncService to work properly

        // Act
        services.AddSyncerbell(_ =>
        {
        })
        .AddSyncerbellInMemoryPersistence();

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        Assert.NotNull(serviceProvider.GetService<ISyncService>());
        Assert.NotNull(serviceProvider.GetService<EntitySyncResolver>());
        Assert.NotNull(serviceProvider.GetService<SyncerbellOptions>());
    }

    [Fact]
    public void AddSyncerbell_WithEntities_ShouldRegisterEntitySyncs()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(); // Required for ISyncService to work properly

        // Act
        services.AddSyncerbell(options =>
        {
            options.AddEntity("NonGenericEntity", typeof(NonGenericEntitySync));
            options.AddEntity<GenericEntity, GenericEntitySync>();
        })
        .AddSyncerbellInMemoryPersistence();

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        Assert.NotNull(serviceProvider.GetService<ISyncService>());
        Assert.NotNull(serviceProvider.GetService<EntitySyncResolver>());
        Assert.NotNull(serviceProvider.GetService<SyncerbellOptions>());
        Assert.NotNull(serviceProvider.GetService<NonGenericEntitySync>());
        Assert.NotNull(serviceProvider.GetService<GenericEntitySync>());
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private class GenericEntity;

    // ReSharper disable once ClassNeverInstantiated.Local
    private class GenericEntitySync : IEntitySync
    {
        public Task<SyncResult> Run(SyncTrigger trigger, SyncEntityOptions entityOptions, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    private class NonGenericEntitySync : IEntitySync
    {
        public Task<SyncResult> Run(SyncTrigger trigger, SyncEntityOptions entityOptions, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
