# Entity Framework Core support for Syncerbell

## Configuration

```c#
services.AddSyncerbellEntityFrameworkCorePersistence(options => 
{
    options.WithDbContextOptions(dbContextOptions =>
    {
        dbContextOptions.UseSqlServer()
            .EnableRetryOnFailure();
    });
});
```

## Table Schema

```tsql
CREATE TABLE [dbo].[SyncLogEntries] (
    Id int NOT NULL IDENTITY(1, 1),
    Entity varchar(100) NOT NULL,
    ParametersJson nvarchar(100) NULL,
    SyncStatus int NOT NULL CONSTRAINT [DF_SyncLogEntries_SyncStatus] DEFAULT 1,
    CreatedAt datetime2 NOT NULL CONSTRAINT [DF_SyncLogEntries_CreatedAt] DEFAULT GETUTCDATE(),
    LeasedAt datetime2 NULL,
    LeaseExpiresAt datetime2 NULL,
    LeasedBy varchar(100) NULL,
    FinishedAt datetime2 NULL,
    ResultMessage nvarchar(MAX) NULL,
    HighWaterMark varchar(100) NULL,
    RowVersion rowversion,
    CONSTRAINT [PK_SyncLogEntries] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [CK_SyncLogEntries_LeasedAt_LeaseExpiresAt] CHECK (
        ([LeasedAt] IS NULL AND [LeaseExpiresAt] IS NULL) OR
        ([LeasedAt] IS NOT NULL AND [LeaseExpiresAt] IS NOT NULL)
    ),
)
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SyncLogEntries_Entity_ParametersJson_SyncStatus]
    ON [dbo].[SyncLogEntries]([Entity], [ParametersJson], [SyncStatus])
    WHERE [SyncStatus] IN (1, 2)
GO

```
