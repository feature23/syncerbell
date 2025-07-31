# Syncerbell

A sync orchestration library for .NET.

## Core Concepts

Syncerbell is a library designed to help you synchronize entities between
your application and a remote service or database. It provides a simple and
flexible way to define synchronization logic, manage the state of entities,
and handle the persistence of synchronization logs.

Syncerbell is built around the concept of "entities" that need to be
synchronized. An entity is an abstract concept that represents a piece of
data that needs to be kept in sync with a remote service or database. This
could be a database record, a file, or any other piece of data that needs to
be synchronized. It could even be an entire database, or a collection of
entities that need to be synchronized together.

Syncerbell is not aware of any external concerns like HTTP requests,
your app's database, or any other external service. It is designed to just
be an orchestration layer that manages the synchronization process,
calling your custom synchronization logic when needed, and keeping track of
the state of entities and their synchronization history.

Syncerbell uses a concept called "eligibility" to determine when an entity
is ready to be synchronized. An entity is eligible for synchronization when
it meets certain criteria defined by the developer. This could be based on
time intervals, changes in the entity's state, or any other custom logic.
Syncerbell provides two built-in eligibility strategies:
- `IntervalEligibilityStrategy`: This strategy allows you to specify a time
  interval after which the entity is considered eligible for synchronization.
- `AlwaysEligibleStrategy`: This strategy makes the entity always eligible for
  synchronization, meaning it will be synchronized every time the sync service
  runs.

You can also implement your own custom eligibility strategies by
implementing the `IEligibilityStrategy` interface. This allows you to define
custom logic for determining when an entity is eligible for synchronization.

Syncerbell also provides a persistence mechanism for storing the state of
entities and their synchronization history. See the 
[persistence section](#persistence-configuration) for more details on how to 
configure the persistence mechanism.

Each sync run for an entity is represented by an `ISyncLogEntry` instance, which
contains information about the synchronization run, such as the entity being
synchronized, the time of the run, the result of the synchronization, and any
additional data that may be relevant to the synchronization process. Syncerbell
automatically manages the creation and storage of these log entries, allowing
you to focus on the synchronization logic itself. You do not normally need
to interact with these log entries directly, but they can be useful for
debugging and monitoring purposes.

Syncerbell supports progress reporting for long-running synchronization
operations. You can report progress using the `ReportProgress` method
on the `EntitySyncContext` provided to your synchronization logic. This progress
will automatically be persisted in the sync log entries.

Some sync operations may take a long time to complete, and Syncerbell
provides a way to handle these long-running operations by allowing you to
"fan out" the synchronization of an entity into multiple smaller
synchronization operations. This is done by calling the `ISyncQueueService` to 
create pending sync log entries for all configured entities, enqueueing the 
resulting log entries (which is up to you to implement), and then for each one, 
calling the `ISyncService.SyncEntityIfEligible` method to process just that one 
entity. This allows you to break down large synchronization operations into
smaller, manageable pieces, while still keeping track of the overall synchronization 
state of the entity. Note that this currently will not consider eligibility
when determining whether to enqueue the entity, because that will be done when
the `SyncEntityIfEligible` method is called. This means that you can enqueue
entities that are not currently eligible for synchronization, and in case they
become eligible before the next sync run, they will be processed as part of that run.

## Installation

The easiest and most common way to get started with Syncerbell is to install
the Entity Framework Core package via NuGet. You can do this using the
command line or by adding it to your project file. This package will
transitively install the core Syncerbell library, and is suitable for use
in most applications.

```bash
dotnet add package F23.Syncerbell.EntityFrameworkCore
```

Alternatively, you can install the core Syncerbell library directly if you
are not using Entity Framework Core, or if you want to use a different
persistence mechanism. This package is suitable for use in most applications.

```bash
dotnet add package F23.Syncerbell
```

After installing the package, there are three main things you need to do:
1. [Configure](#configuration) Syncerbell in your application.
2. Set up the [hosting](#hosting) mechanism for Syncerbell.
3. Implement the [synchronization logic](#synchronization-logic) for your entities.

## Configuration

Syncerbell must be configured in your application to work correctly. The
configuration is typically done in the `Program.cs` file of your application,
and is designed to work well with `Microsoft.Extensions.DependencyInjection`.

### Core Configuration

You must first add the Syncerbell services to your service collection, and 
configure at least one entity type to be synchronized. This is done using
the `AddSyncerbell` method.

```c#
services.AddSyncerbell(options =>
{
    options.AddEntity<ToDoItem, MockToDoItemSync>(entity =>
    {
        // sync every hour
        entity.Eligibility = new IntervalEligibilityStrategy(TimeSpan.FromHours(1));
    });
});
```

For more advanced use cases of statically-registered entities, you can
see the documentation for the non-generic `AddEntity` overload.

If you need to generate entities to sync at runtime (such as in a multi-tenant application),
you can register a custom `IEntityProvider` implementation.

### Persistence Configuration

Syncerbell supports multiple persistence mechanisms. The most common is
Entity Framework Core, but you can also use custom persistence mechanisms
by implementing the `ISyncLogPersistence` interface.

#### In-Memory

For testing or development purposes, you can use the in-memory persistence
mechanism. This is not suitable for production use, but can be useful for
quick prototyping or testing.

```c#
services.AddSyncerbellInMemoryPersistence();
```

#### Entity Framework Core

To use Entity Framework Core as the persistence mechanism, you need to
add the `F23.Syncerbell.EntityFrameworkCore` package and configure it in your
`Program.cs` file. This is the most common and recommended way to use Syncerbell
in production applications.

```c#
services.AddSyncerbellEntityFrameworkCorePersistence(options =>
{
    options.ConfigureDbContextOptions = dbContextOptions =>
    {
        // configure the EF Core context like you normally would
        dbContextOptions.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionString"),
            sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure();
            });
    };
});
```

Currently, there are no migrations for the Syncerbell database schema.
You can find the schema for the `SyncLogEntries` table in the 
[Syncerbell.EntityFrameworkCore project README](Syncerbell.EntityFrameworkCore/README.md).

## Hosting

Syncerbell is designed to be hosted as a background service of some kind.
You can easily use Syncerbell in Azure Functions, Windows or Linux services,
ASP.NET Core applications as a background service, or any other
background service host.

Note that Syncerbell should be called periodically to check for
entities that are eligible for synchronization. The frequency of these checks
should be at least as frequent as your smallest eligibility interval, but not
so frequent that it causes performance issues or excessive load on your
application or the remote service you are synchronizing with.

### Azure Functions

To use Syncerbell in Azure Functions, you can create a function that
triggers on a schedule or an event, and then use the `Syncerbell` library
to perform the synchronization.

```c#
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Syncerbell;

namespace MyApp.Functions;

public class SyncerbellTimer(ISyncService syncService, ILogger<SyncerbellTimer> logger)
{
    private const bool RunOnStartup =
        #if DEBUG
        true;
        #else
        false;
        #endif

    [Function(nameof(SyncerbellTimer))]
    public async Task Run([TimerTrigger("0 0 * * * *", RunOnStartup = RunOnStartup)] TimerInfo timerInfo)
    {
        logger.LogInformation("Syncerbell timer function triggered at: {Time}", DateTime.UtcNow);

        try
        {
            await syncService.SyncAllEligible(SyncTriggerType.Timer);
            logger.LogInformation("Syncerbell timer completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during Syncerbell timer execution");
            throw;
        }
    }
}
```

### Hosted Service

To use Syncerbell in an ASP.NET Core application, Windows or Linux service, or
any other command-line-invoked application, you can create a hosted service
using the HostedService library:

```bash
dotnet add package F23.Syncerbell.HostedService
```

Then, you can register the hosted service in your `Program.cs`:
```c#
services.AddSyncerbellHostedService(options =>
{
    // NOTE: This is purposefully very short to demonstrate the library.
    options.StartupDelay = TimeSpan.FromSeconds(5);
    options.CheckInterval = TimeSpan.FromSeconds(30);
});
```

See the `ExampleApiUsage` project for an example of how to configure a hosted service.

## Synchronization Logic

To implement the synchronization logic for your entities, you need to create
a class that implements the `IEntitySync` interface. This class will contain the
logic for synchronizing the entity with the remote service or database.

This interface defines a `Run` method that will be called by Syncerbell
when the entity is eligible for synchronization. You can implement this method
to perform the actual synchronization logic, such as making HTTP requests,
updating the database, or any other necessary operations.

It will provide you with a context object that contains information about the
entity being synchronized, and prior run details (such as the last "high water mark" for the entity).
Your implementation should return a `SyncResult` object that indicates
whether the synchronization was successful, and an optional "high water mark"
that can be used for incremental synchronization in the future sync runs.
