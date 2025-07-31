using Microsoft.Extensions.Logging;
using Syncerbell;

namespace ExampleInMemoryQueue;

public class OrderSync(ILogger<OrderSync> logger) : IEntitySync
{
    public async Task<SyncResult> Run(EntitySyncContext context, CancellationToken cancellationToken = default)
    {
        var threadId = Thread.CurrentThread.ManagedThreadId;
        logger.LogInformation("OrderSync starting on thread {ThreadId} for entity {EntityType}",
            threadId, context.Entity.Entity);

        const int totalOperations = 4;
        await context.ReportProgress(0, totalOperations);

        for (int i = 1; i <= totalOperations; i++)
        {
            logger.LogInformation("OrderSync thread {ThreadId}: Processing operation {Operation} of {Total}",
                threadId, i, totalOperations);

            // Simulate order processing synchronization work
            await Task.Delay(Random.Shared.Next(400, 1200), cancellationToken);

            await context.ReportProgress(i, totalOperations);
        }

        logger.LogInformation("OrderSync completed on thread {ThreadId} for entity {EntityType}",
            threadId, context.Entity.Entity);

        return new SyncResult(Entity: context.Entity, Success: true);
    }
}
