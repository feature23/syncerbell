using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Syncerbell.EntityFrameworkCore;

/// <summary>
/// Represents a log entry for synchronization operations.
/// <para />
/// This class is a private implementation detail of this library and is not intended for direct use by consumers.
/// Instead, use the <see cref="ISyncLogPersistence"/> interface to interact with sync logs via their <see cref="ISyncLogEntry"/> interface.
/// </summary>
public class SyncLogEntry : ISyncLogEntry
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    [StringLength(maximumLength: 100)]
    [Unicode(false)]
    public required string Entity { get; init; }

    [StringLength(maximumLength: 500)]
    public string? ParametersJson { get; init; }

    public SyncStatus SyncStatus { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public DateTime? LeasedAt { get; set; }

    public DateTime? LeaseExpiresAt { get; set; }

    [StringLength(maximumLength: 100)]
    [Unicode(false)]
    public string? LeasedBy { get; set; }

    public DateTime? FinishedAt { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string? ResultMessage { get; set; }

    [StringLength(maximumLength: 100)]
    [Unicode(false)]
    public string? HighWaterMark { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;
}
