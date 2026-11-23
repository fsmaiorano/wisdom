using Wisdom.BuildingBlocks.Events.IntegrationEvents.Contracts;
using Wisdom.BuildingBlocks.Loggers;
using Wisdom.MediaWorker.Application.Abstractions;

namespace Wisdom.MediaWorker.Application.Services;

/// <summary>
/// Full video pipeline for a <see cref="MediaReadyIntegrationEvent"/>:
/// download → transcode → upload → mark ready.
/// Failures are recorded as status "Failed" and never thrown, so the message is
/// acknowledged and the reconciler picks the row up later.
/// </summary>
public class MediaProcessor(
    IMediaRepository repository,
    IObjectStorage storage,
    IVideoTranscoder transcoder,
    IAppLogger<MediaProcessor> logger)
{
    public async Task ProcessAsync(MediaReadyIntegrationEvent @event, CancellationToken ct = default)
    {
        logger.LogInformation(
            "Received MediaReadyIntegrationEvent: MediaId={MediaId} LessonId={LessonId} MediaType={MediaType} FilePath={FilePath}",
            @event.MediaId, @event.LessonId, @event.MediaType, @event.FilePath);

        if (!Guid.TryParse(@event.MediaId, out var mediaId))
        {
            logger.LogError("Invalid MediaId '{MediaId}' — ignoring event", @event.MediaId);
            return;
        }

        try
        {
            await repository.SetStatusAsync(mediaId, MediaStatuses.Processing, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "media {MediaId}: failed to set Processing status", mediaId);
            return;
        }

        try
        {
            await RunPipelineAsync(mediaId, @event.FilePath, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "media {MediaId}: processing failed", mediaId);
            try
            {
                await repository.SetStatusAsync(mediaId, MediaStatuses.Failed, CancellationToken.None);
            }
            catch (Exception statusEx)
            {
                logger.LogError(statusEx, "media {MediaId}: failed to set Failed status", mediaId);
            }
        }
    }

    private async Task RunPipelineAsync(Guid mediaId, string filePath, CancellationToken ct)
    {
        var rawPath = Path.Combine(Path.GetTempPath(), $"wisdom-raw-{Guid.NewGuid():N}{Path.GetExtension(filePath)}");

        try
        {
            await storage.DownloadToFileAsync(filePath, rawPath, ct);

            using var output = await transcoder.ProcessAsync(rawPath, ct);

            var prefix = $"media/{mediaId}";
            var thumbnailKey = $"{prefix}/thumbnail.jpg";
            await storage.UploadFileAsync(thumbnailKey, output.ThumbnailPath, "image/jpeg", ct);

            foreach (var (label, localPath) in output.Videos)
            {
                var key = $"{prefix}/{label}.mp4";
                await storage.UploadFileAsync(key, localPath, "video/mp4", ct);
                logger.LogInformation("media {MediaId}: uploaded {Key}", mediaId, key);
            }

            await repository.MarkReadyAsync(mediaId, thumbnailKey, output.DurationSeconds, ct);

            try
            {
                await storage.DeleteAsync(filePath, ct);
                logger.LogInformation("media {MediaId}: deleted raw file {FilePath}", mediaId, filePath);
            }
            catch (Exception ex)
            {
                logger.LogWarning("media {MediaId}: failed to delete raw file {FilePath}: {Error}", mediaId, filePath,
                    ex.Message);
            }

            logger.LogInformation("media {MediaId} is Ready (duration={Duration}s, thumbnail={Thumbnail})", mediaId,
                output.DurationSeconds, thumbnailKey);
        }
        finally
        {
            if (File.Exists(rawPath))
                File.Delete(rawPath);
        }
    }
}
