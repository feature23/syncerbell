using Microsoft.Extensions.Logging;
using Syncerbell;

namespace ExampleInMemoryQueue;

public class ProductSync(ILogger<ProductSync> logger) : IEntitySync
{
    public async Task<SyncResult> Run(EntitySyncContext context, CancellationToken cancellationToken = default)
    {
        var threadId = Thread.CurrentThread.ManagedThreadId;
        logger.LogInformation("ProductSync starting on thread {ThreadId} for entity {EntityType}",
            threadId, context.Entity.Entity);

        const int totalOperations = 7;
        await context.ReportProgress(0, totalOperations);

        for (int i = 1; i <= totalOperations; i++)
        {
            logger.LogInformation("ProductSync thread {ThreadId}: Processing operation {Operation} of {Total}",
                threadId, i, totalOperations);

            // Simulate product inventory synchronization work
            await Task.Delay(Random.Shared.Next(300, 1000), cancellationToken);

            await context.ReportProgress(i, totalOperations);
        }

        logger.LogInformation("ProductSync completed on thread {ThreadId} for entity {EntityType}",
            threadId, context.Entity.Entity);

        return new SyncResult(Entity: context.Entity, Success: true);
    }
}
