namespace Syncerbell.HostedService;

/// <summary>
/// Provides options for configuring the Syncerbell hosted service, including startup delay and sync check interval.
/// </summary>
public class SyncerbellHostedServiceOptions
{
    /// <summary>
    /// Gets or sets the delay before the hosted service starts its first sync operation.
    /// </summary>
    public TimeSpan StartupDelay { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// Gets or sets the interval between periodic sync checks.
    /// </summary>
    public TimeSpan CheckInterval { get; set; } = TimeSpan.FromMinutes(1);
}
