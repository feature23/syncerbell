using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Syncerbell.HostedService;

/// <summary>
/// Provides a hosted service that periodically triggers synchronization for all eligible entities using Syncerbell.
/// </summary>
/// <param name="syncService">The sync service used to perform synchronization operations.</param>
/// <param name="options">The options for configuring the hosted service's scheduling and behavior.</param>
/// <param name="logger">The logger used for diagnostic and operational messages.</param>
public class SyncerbellHostedService(
    ISyncService syncService,
    SyncerbellHostedServiceOptions options,
    ILogger<SyncerbellHostedService> logger)
    : IHostedService
{
    private Timer? _timer;
    private CancellationTokenSource? _cts;

    /// <summary>
    /// Starts the Syncerbell hosted service and schedules periodic sync operations.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A completed <see cref="Task"/>.</returns>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting Syncerbell Hosted Service ({StartupDelay} delay, {Interval} interval)",
            options.StartupDelay, options.CheckInterval);

        _cts = new CancellationTokenSource();
        _timer = new Timer(TimerTick,
            state: null,
            dueTime: (int)options.StartupDelay.TotalMilliseconds,
            period: (int)options.CheckInterval.TotalMilliseconds);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Stops the Syncerbell hosted service and cancels any scheduled sync operations.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A completed <see cref="Task"/>.</returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping Syncerbell Hosted Service");
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
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
            syncService.SyncAllEligible(SyncTriggerType.Timer, _cts?.Token ?? CancellationToken.None)
                .GetAwaiter()
                .GetResult(); // Blocking call to ensure we handle exceptions properly
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during Syncerbell Hosted Service timer tick");
        }
    }
}
