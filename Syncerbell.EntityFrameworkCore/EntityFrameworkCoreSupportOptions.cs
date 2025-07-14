using Microsoft.EntityFrameworkCore;

namespace Syncerbell.EntityFrameworkCore;

/// <summary>
/// Provides options for configuring Entity Framework Core support in Syncerbell, including schema, table names, and DbContext options.
/// </summary>
public class EntityFrameworkCoreSupportOptions
{
    /// <summary>
    /// Gets or sets the delegate to configure <see cref="DbContextOptionsBuilder"/> for the sync log context.
    /// </summary>
    public Action<DbContextOptionsBuilder>? ConfigureDbContextOptions { get; set; }

    /// <summary>
    /// Gets or sets the database schema to use for sync log tables. Defaults to "dbo".
    /// </summary>
    public string Schema { get; set; } = "dbo";

    /// <summary>
    /// Gets or sets the table name for sync log entries. Defaults to "SyncLogEntries".
    /// </summary>
    public string SyncLogEntriesTableName { get; set; } = "SyncLogEntries";

    /// <summary>
    /// Sets the delegate to configure <see cref="DbContextOptionsBuilder"/> and returns the current options instance.
    /// </summary>
    /// <param name="configureDbContextOptions">The delegate to configure the DbContext options.</param>
    /// <returns>The current <see cref="EntityFrameworkCoreSupportOptions"/> instance.</returns>
    public EntityFrameworkCoreSupportOptions WithDbContextOptions(
        Action<DbContextOptionsBuilder> configureDbContextOptions)
    {
        ConfigureDbContextOptions = configureDbContextOptions;
        return this;
    }
}
