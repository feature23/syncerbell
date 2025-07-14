using Microsoft.EntityFrameworkCore;

namespace Syncerbell.EntityFrameworkCore;

/// <summary>
/// Represents the Entity Framework Core database context for storing and managing sync log entries in Syncerbell.
/// </summary>
/// <param name="contextOptions">The options to be used by the DbContext.</param>
/// <param name="efOptions">The options for configuring Entity Framework Core support in Syncerbell.</param>
public class SyncLogDbContext(
    DbContextOptions contextOptions,
    EntityFrameworkCoreSupportOptions efOptions)
    : DbContext(contextOptions)
{
    /// <summary>
    /// Gets the <see cref="DbSet{TEntity}"/> for sync log entries.
    /// </summary>
    public DbSet<SyncLogEntry> SyncLogEntries => Set<SyncLogEntry>();

    /// <summary>
    /// Configures the model for the sync log entries, including table name and schema.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SyncLogEntry>(e => e.ToTable(efOptions.SyncLogEntriesTableName, efOptions.Schema));
    }
}
