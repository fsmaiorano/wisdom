using Wisdom.BuildingBlocks.Events.IntegrationEvents.Contracts;
using Wisdom.BuildingBlocks.Loggers;
using Wisdom.MediaWorker.Application.Abstractions;

namespace Wisdom.MediaWorker.Application.Services;

/// <summary>
/// Re-dispatches media rows stuck in "Uploaded" or "Failed" through the normal pipeline.
/// </summary>
public class MediaReconciler(
    IMediaRepository repository,
    MediaProcessor processor,
    IAppLogger<MediaReconciler> logger)
{
    public async Task RunOnceAsync(TimeSpan staleAfter, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow - staleAfter;
        var claimed = await repository.ClaimStaleAsync(cutoff, ct);

        if (claimed.Count == 0)
            return;

        logger.LogInformation("Reconciler: dispatching {Count} stale media entries for reprocessing", claimed.Count);

        foreach (var media in claimed)
        {
            logger.LogInformation("Reconciler: retrying media {MediaId} (filePath={FilePath})", media.Id,
                media.FilePath);

            await processor.ProcessAsync(new MediaReadyIntegrationEvent
            {
                MediaId = media.Id.ToString(),
                LessonId = media.LessonId.ToString(),
                MediaType = media.Type,
                FilePath = media.FilePath
            }, ct);
        }
    }
}
