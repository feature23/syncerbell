using ExampleInMemoryQueue;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Syncerbell;
using System.Linq;

// Create service collection and configure services
var services = new ServiceCollection();

// Add logging
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// Configure Syncerbell with multiple entities
services.AddSyncerbell(options =>
{
    options.DefaultLeaseExpiration = TimeSpan.FromMinutes(30);

    // Add Customer entity
    options.AddEntity<Customer, CustomerSync>(entity =>
    {
        entity.LeaseExpiration = TimeSpan.FromMinutes(15);
        entity.Parameters = new SortedDictionary<string, object?>()
        {
            ["Region"] = "US-West",
            ["BatchSize"] = 100
        };
        entity.Eligibility = new AlwaysEligibleStrategy();
    });

    // Add Product entity
    options.AddEntity<Product, ProductSync>(entity =>
    {
        entity.LeaseExpiration = TimeSpan.FromMinutes(20);
        entity.Parameters = new SortedDictionary<string, object?>()
        {
            ["Category"] = "Electronics",
            ["BatchSize"] = 50
        };
        entity.Eligibility = new AlwaysEligibleStrategy();
    });

    // Add Order entity
    options.AddEntity<Order, OrderSync>(entity =>
    {
        entity.LeaseExpiration = TimeSpan.FromMinutes(10);
        entity.Parameters = new SortedDictionary<string, object?>()
        {
            ["Status"] = "Active",
            ["BatchSize"] = 25
        };
        entity.Eligibility = new AlwaysEligibleStrategy();
    });
});

// Add in-memory persistence for sync logs
services.AddSyncerbellInMemoryPersistence();

// Build service provider
var serviceProvider = services.BuildServiceProvider();

// Get required services
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
var syncQueueService = serviceProvider.GetRequiredService<ISyncQueueService>();
var syncService = serviceProvider.GetRequiredService<ISyncService>();

logger.LogInformation("=== Syncerbell Threading Example ===");
logger.LogInformation("This example demonstrates fanning out queue operations using multiple threads");

try
{
    // Step 1: Create queued sync entries for all entities
    logger.LogInformation("Step 1: Creating queued sync entries for all entities...");
    var queuedEntries = await syncQueueService.CreateAllQueuedSyncEntries(SyncTriggerType.Manual);

    logger.LogInformation("Created {Count} queued sync entries:", queuedEntries.Count);
    foreach (var entry in queuedEntries)
    {
        logger.LogInformation("  - Entry ID: {EntryId}, Entity: {EntityType}", entry.Id, entry.Entity);
    }

    // Step 2: Simulate recording queue message IDs (as if we put them in a real queue)
    logger.LogInformation("Step 2: Recording queue message IDs...");
    var queueMessageTasks = queuedEntries.Select(async entry =>
    {
        var queueMessageId = $"msg_{Guid.NewGuid():N}";
        await syncQueueService.RecordQueueMessageId(entry.Id, queueMessageId);
        logger.LogInformation("Recorded queue message ID {MessageId} for entry {EntryId}", queueMessageId, entry.Id);
        return (entry, queueMessageId);
    });

    var entriesWithMessages = await Task.WhenAll(queueMessageTasks);

    // Step 3: Process sync entries concurrently using multiple threads
    logger.LogInformation("Step 3: Processing sync entries concurrently using multiple threads...");
    logger.LogInformation("Starting {Count} threads for parallel processing...", queuedEntries.Count);

    var processingTasks = entriesWithMessages.Select(async entryInfo =>
    {
        var (entry, _) = entryInfo;
        var threadId = Environment.CurrentManagedThreadId;

        logger.LogInformation("Thread {ThreadId}: Starting to process entry {EntryId} (Entity: {EntityType})",
            threadId, entry.Id, entry.Entity);

        try
        {
            // Use ISyncService to sync the individual entity from the queued entry
            var result = await syncService.SyncEntityIfEligible(entry.Id, SyncTriggerType.Manual);

            if (result != null)
            {
                logger.LogInformation("Thread {ThreadId}: Successfully processed entry {EntryId} - {EntityType}",
                    threadId, entry.Id, entry.Entity);
                return result;
            }
            else
            {
                logger.LogWarning("Thread {ThreadId}: Failed to process entry {EntryId} - result was null",
                    threadId, entry.Id);
                return null;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Thread {ThreadId}: Error processing entry {EntryId}",
                threadId, entry.Id);
            return null;
        }
    });

    // Wait for all threads to complete
    var results = await Task.WhenAll(processingTasks);

    logger.LogInformation("=== Processing Complete ===");

    var successfulResults = results.Where(r => r?.Success == true).ToList();
    var failedResults = results.Where(r => r == null || r.Success == false).ToList();

    logger.LogInformation("Successfully processed: {SuccessCount} entities", successfulResults.Count);
    logger.LogInformation("Failed to process: {FailCount} entities", failedResults.Count);

    if (successfulResults.Any())
    {
        logger.LogInformation("Successful sync results:");
        foreach (var result in successfulResults)
        {
            logger.LogInformation("  - {EntityType}: Success", result?.Entity?.EntityType?.Name ?? "null");
        }
    }

    logger.LogInformation("Example completed successfully!");
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occurred during the example execution");
}
finally
{
    // Dispose the service provider
    serviceProvider.Dispose();
}
