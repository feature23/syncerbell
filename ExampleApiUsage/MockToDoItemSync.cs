using Microsoft.Extensions.Logging;
using Syncerbell;

namespace ExampleApiUsage;

public class MockToDoItemSync(ILogger<MockToDoItemSync> logger) : IEntitySync
{
    public async Task<SyncResult> Run(EntitySyncContext context, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("MockToDoItemSync called, pretending to sync");

        // Simulate processing multiple items
        const int totalItems = 10;

        // Report initial progress
        await context.ReportProgress(0, totalItems);

        for (int i = 1; i <= totalItems; i++)
        {
            logger.LogDebug("Processing item {ItemNumber} of {TotalItems}", i, totalItems);

            // Simulate work for each item
            await Task.Delay(500, cancellationToken);

            // Report progress after each item
            await context.ReportProgress(i, totalItems);

            logger.LogDebug("Completed item {ItemNumber}, progress: {Progress:P}", i, (float)i / totalItems);
        }

        logger.LogInformation("MockToDoItemSync completed \"successfully\"");

        return new SyncResult(Entity: context.Entity, Success: true);
    }
}
