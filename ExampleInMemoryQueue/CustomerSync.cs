using Microsoft.Extensions.Logging;
using Syncerbell;

namespace ExampleInMemoryQueue;

public class CustomerSync(ILogger<CustomerSync> logger) : IEntitySync
{
    public async Task<SyncResult> Run(EntitySyncContext context, CancellationToken cancellationToken = default)
    {
        var threadId = Thread.CurrentThread.ManagedThreadId;
        logger.LogInformation("CustomerSync starting on thread {ThreadId} for entity {EntityType}",
            threadId, context.Entity.Entity);

        const int totalOperations = 5;
        await context.ReportProgress(0, totalOperations);

        for (int i = 1; i <= totalOperations; i++)
        {
            logger.LogInformation("CustomerSync thread {ThreadId}: Processing operation {Operation} of {Total}",
                threadId, i, totalOperations);

            // Simulate customer data synchronization work
            await Task.Delay(Random.Shared.Next(500, 1500), cancellationToken);

            await context.ReportProgress(i, totalOperations);
        }

        logger.LogInformation("CustomerSync completed on thread {ThreadId} for entity {EntityType}",
            threadId, context.Entity.Entity);

        return new SyncResult(Entity: context.Entity, Success: true);
    }
}
