using Microsoft.Extensions.Options;
using Wisdom.BuildingBlocks.BackgroundServices;
using Wisdom.BuildingBlocks.Loggers;
using Wisdom.MediaWorker.Application.Services;
using Wisdom.MediaWorker.Application.Settings;
namespace Wisdom.MediaWorker.Worker.Workers;

public class MediaReconcilerWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<ReconcilerSettings> settings,
    IAppLogger<MediaReconcilerWorker> logger)
    : PeriodicBackgroundService<MediaReconcilerWorker>(TimeSpan.FromSeconds(settings.Value.IntervalSeconds), logger)
{
    protected override async Task ExecuteOnceAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();
        var reconciler = scope.ServiceProvider.GetRequiredService<MediaReconciler>();
        await reconciler.RunOnceAsync(TimeSpan.FromSeconds(settings.Value.StaleAfterSeconds), stoppingToken);
    }
}
