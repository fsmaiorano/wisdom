using Microsoft.Extensions.Hosting;
using Wisdom.BuildingBlocks.Loggers;
namespace Wisdom.BuildingBlocks.BackgroundServices;

public abstract class PeriodicBackgroundService<T>(TimeSpan interval, IAppLogger<T> logger) : BackgroundService
{
    protected abstract Task ExecuteOnceAsync(CancellationToken stoppingToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(interval);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ExecuteOnceAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Error executing periodic work");
                }

                if (!await timer.WaitForNextTickAsync(stoppingToken))
                    break;
            }
        }
        catch (OperationCanceledException) { }
    }
}