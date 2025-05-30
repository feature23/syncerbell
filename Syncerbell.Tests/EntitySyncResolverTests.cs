using Microsoft.Extensions.DependencyInjection;

namespace Syncerbell.Tests;

public class EntitySyncResolverTests
{
    [Fact]
    public void Resolve_ShouldReturnEntitySync_WhenTypeIsRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<TestEntitySync>();
        var provider = services.BuildServiceProvider();

        var resolver = new EntitySyncResolver(provider);

        var options = new SyncEntityOptions("TestEntity", typeof(TestEntitySync));

        // Act
        var entitySync = resolver.Resolve(options);

        // Assert
        Assert.NotNull(entitySync);
        Assert.IsType<TestEntitySync>(entitySync);
    }

    [Fact]
    public void Resolve_ShouldThrow_WhenTypeIsNotRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        var provider = services.BuildServiceProvider();

        var resolver = new EntitySyncResolver(provider);

        var options = new SyncEntityOptions("TestEntity", typeof(TestEntitySync));

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => resolver.Resolve(options));
    }

    private class TestEntitySync : IEntitySync
    {
        public Task<SyncResult> Run(SyncTrigger trigger, SyncEntityOptions entityOptions, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
