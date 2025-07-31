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
    /// Gets the type of trigger that initiated the sync operation.
    /// </summary>
    public required SyncTriggerType TriggerType { get; init; }

    /// <inheritdoc />
    public SyncStatus SyncStatus { get; set; }

    /// <inheritdoc />
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <inheritdoc />
    public DateTime? LeasedAt { get; set; }

    /// <inheritdoc />
    public DateTime? LeaseExpiresAt { get; set; }

    /// <inheritdoc />
    [StringLength(maximumLength: 100)]
    [Unicode(false)]
    public string? LeasedBy { get; set; }

    /// <inheritdoc />
    [StringLength(maximumLength: 100)]
    [Unicode(false)]
    public string? QueueMessageId { get; set; }

    /// <inheritdoc />
    public DateTime? QueuedAt { get; set; }

    /// <inheritdoc />
    public DateTime? FinishedAt { get; set; }

    /// <inheritdoc />
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string? ResultMessage { get; set; }

    /// <inheritdoc />
    [StringLength(maximumLength: 100)]
    [Unicode(false)]
    public string? HighWaterMark { get; set; }

    /// <inheritdoc />
    public int? ProgressValue { get; set; }

    /// <inheritdoc />
    public int? ProgressMax { get; set; }

    /// <inheritdoc />
    public int? RecordCount { get; set; }

    /// <inheritdoc />
    [NotMapped]
    string ISyncLogEntry.Id => Id.ToString();

    /// <summary>
    /// Gets or sets the row version for concurrency checking.
    /// </summary>
    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;
}
