namespace Syncerbell;

/// <summary>
/// Attribute to specify the schema version for a type. Increment the version whenever the schema changes.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
public sealed class SchemaVersionAttribute : Attribute
{
    /// <summary>
    /// Gets the schema version.
    /// </summary>
    public int Version { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaVersionAttribute"/> class.
    /// </summary>
    /// <param name="version">The integer version of the schema.</param>
    public SchemaVersionAttribute(int version)
    {
        Version = version;
    }
}
