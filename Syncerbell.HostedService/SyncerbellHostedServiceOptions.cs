namespace Syncerbell.HostedService;

public class SyncerbellHostedServiceOptions
{
    public TimeSpan StartupDelay { get; set; } = TimeSpan.Zero;

    public TimeSpan CheckInterval { get; set; } = TimeSpan.FromMinutes(1);
}
