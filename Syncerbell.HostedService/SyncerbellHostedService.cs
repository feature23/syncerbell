using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Syncerbell.HostedService;

public class SyncerbellHostedService(
    ISyncService syncService,
    SyncerbellHostedServiceOptions options,
    ILogger<SyncerbellHostedService> logger)
    : IHostedService
{
    private Timer? _timer;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting Syncerbell Hosted Service ({StartupDelay} delay, {Interval} interval)",
            options.StartupDelay, options.CheckInterval);

        _timer = new Timer(TimerTick,
            state: null,
            dueTime: (int)options.StartupDelay.TotalMilliseconds,
            period: (int)options.CheckInterval.TotalMilliseconds);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping Syncerbell Hosted Service");

        _timer?.Change(Timeout.Infinite, Timeout.Infinite);
        _timer?.Dispose();
        _timer = null;

        return Task.CompletedTask;
    }

    private void TimerTick(object? state)
    {
        logger.LogDebug("Syncerbell Hosted Service timer ticked at {Time}", DateTime.UtcNow);
        try
        {
            syncService.SyncAllEligible(SyncTriggerType.Timer)
                .GetAwaiter()
                .GetResult(); // Blocking call to ensure we handle exceptions properly
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during Syncerbell Hosted Service timer tick");
        }
    }
}
