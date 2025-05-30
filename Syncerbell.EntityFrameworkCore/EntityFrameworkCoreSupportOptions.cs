using Microsoft.EntityFrameworkCore;

namespace Syncerbell.EntityFrameworkCore;

public class EntityFrameworkCoreSupportOptions
{
    public Action<DbContextOptionsBuilder>? ConfigureDbContextOptions { get; set; }

    public string Schema { get; set; } = "dbo";

    public string SyncLogEntriesTableName { get; set; } = "SyncLogEntries";

    public EntityFrameworkCoreSupportOptions WithDbContextOptions(
        Action<DbContextOptionsBuilder> configureDbContextOptions)
    {
        ConfigureDbContextOptions = configureDbContextOptions;
        return this;
    }
}
