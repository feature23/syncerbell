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
    /// <summary>
    /// Gets the unique identifier for the sync log entry.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    /// <summary>
    /// Gets the name of the entity being synchronized.
    /// </summary>
    [StringLength(maximumLength: 100)]
    [Unicode(false)]
    public required string Entity { get; init; }

    /// <summary>
    /// Gets the serialized parameters associated with the entity, if any.
    /// </summary>
    [StringLength(maximumLength: 500)]
    public string? ParametersJson { get; init; }

    /// <summary>
    /// Gets the schema version of the entity, if specified.
    /// </summary>
    public int? SchemaVersion { get; init; }

    /// <summary>
    /// Gets or sets the current status of the sync operation.
    /// </summary>
    public SyncStatus SyncStatus { get; set; }

    /// <summary>
    /// Gets the date and time when the log entry was created.
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the sync operation was leased.
    /// </summary>
    public DateTime? LeasedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the lease for the sync operation expires.
    /// </summary>
    public DateTime? LeaseExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the machine or process that leased the sync operation.
    /// </summary>
    [StringLength(maximumLength: 100)]
    [Unicode(false)]
    public string? LeasedBy { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the sync operation finished.
    /// </summary>
    public DateTime? FinishedAt { get; set; }

    /// <summary>
    /// Gets or sets the result message of the sync operation, if any.
    /// </summary>
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string? ResultMessage { get; set; }

    /// <summary>
    /// Gets or sets the high water mark for the sync operation, if any.
    /// </summary>
    [StringLength(maximumLength: 100)]
    [Unicode(false)]
    public string? HighWaterMark { get; set; }

    /// <summary>
    /// Gets or sets the row version for concurrency checking.
    /// </summary>
    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;
}
