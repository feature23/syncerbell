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
    SchemaVersion int NULL,
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

CREATE UNIQUE NONCLUSTERED INDEX [IX_SyncLogEntries_Entity_ParametersJson_SchemaVersion_SyncStatus]
    ON [dbo].[SyncLogEntries]([Entity], [ParametersJson], [SchemaVersion], [SyncStatus])
    WHERE [SyncStatus] IN (1, 2)
GO

CREATE NONCLUSTERED INDEX [IX_SyncLogEntries_Entity_ParametersJson_SchemaVersion]
    ON [dbo].[SyncLogEntries]([Entity], [ParametersJson], [SchemaVersion])
GO

```

## Migrating from 0.2.0 to 0.3.0

The 0.3.0 release adds the SchemaVersion column to the SyncLogEntries table. 
If you are upgrading from 0.2.0 and not using dacpac deployment, you need to run the following SQL script
to add the new column and index:

```tsql
DROP INDEX IF EXISTS [IX_SyncLogEntries_Entity_ParametersJson_SyncStatus] ON [dbo].[SyncLogEntries];
GO

DROP INDEX IF EXISTS [IX_SyncLogEntries_Entity_ParametersJson] ON [dbo].[SyncLogEntries];
GO

ALTER TABLE [dbo].[SyncLogEntries]
ADD [SchemaVersion] int NULL;
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SyncLogEntries_Entity_ParametersJson_SchemaVersion_SyncStatus]
    ON [dbo].[SyncLogEntries]([Entity], [ParametersJson], [SchemaVersion], [SyncStatus])
    WHERE [SyncStatus] IN (1, 2);
GO

CREATE NONCLUSTERED INDEX [IX_SyncLogEntries_Entity_ParametersJson_SchemaVersion]
    ON [dbo].[SyncLogEntries]([Entity], [ParametersJson], [SchemaVersion]);
GO
```
