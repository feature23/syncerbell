using ExampleApiUsage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Syncerbell;
using Syncerbell.HostedService;

var hostBuilder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(builder =>
    {
        builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    })
    .ConfigureServices(services =>
    {
        services.AddSyncerbell(options =>
        {
            options.DefaultLeaseExpiration = TimeSpan.FromHours(1);

            options.AddEntity<ToDoItem, MockToDoItemSync>(entity =>
            {
                entity.LeaseExpiration = TimeSpan.FromMinutes(10);

                // example parameters for the entity
                entity.Parameters = new SortedDictionary<string, object?>()
                {
                    ["AccountId"] = 12345,
                };

                // NOTE: This is purposefully very short to demonstrate the library.
                entity.Eligibility = new IntervalEligibilityStrategy(TimeSpan.FromMinutes(1));
            });
        });

        // Just for demonstration purposes, this adds in-memory persistence for sync logs.
        services.AddSyncerbellInMemoryPersistence();

        services.AddSyncerbellHostedService(options =>
        {
            // NOTE: This is purposefully very short to demonstrate the library.
            options.StartupDelay = TimeSpan.FromSeconds(5);
            options.CheckInterval = TimeSpan.FromSeconds(30);
        });
    });

var host = hostBuilder.Build();
await host.RunAsync();
