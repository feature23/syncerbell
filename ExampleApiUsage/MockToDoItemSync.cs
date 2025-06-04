using Microsoft.Extensions.Logging;
using Syncerbell;

namespace ExampleApiUsage;

public class MockToDoItemSync(ILogger<MockToDoItemSync> logger) : IEntitySync
{
    public async Task<SyncResult> Run(SyncTrigger trigger, SyncEntityOptions entityOptions, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("MockToDoItemSync called, pretending to sync");

        await Task.Delay(2000, cancellationToken);

        logger.LogInformation("MockToDoItemSync completed \"successfully\"");

        return new SyncResult(Entity: entityOptions, Success: true);
    }
}
