using Microsoft.EntityFrameworkCore;

namespace Syncerbell.EntityFrameworkCore;

public class SyncLogDbContext(
    DbContextOptions contextOptions,
    EntityFrameworkCoreSupportOptions efOptions)
    : DbContext(contextOptions)
{
    public DbSet<SyncLogEntry> SyncLogEntries => Set<SyncLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SyncLogEntry>(e => e.ToTable(efOptions.SyncLogEntriesTableName, efOptions.Schema));
    }
}
