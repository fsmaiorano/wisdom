namespace Wisdom.MediaWorker.Application.Abstractions;

public sealed record StaleMedia(Guid Id, Guid LessonId, string Type, string FilePath);

public interface IMediaRepository
{
    Task SetStatusAsync(Guid mediaId, string status, CancellationToken ct = default);

    Task MarkReadyAsync(Guid mediaId, string thumbnailPath, int durationSeconds, CancellationToken ct = default);

    /// <summary>
    /// Atomically moves rows stuck in Uploaded/Failed since before <paramref name="cutoff"/> to
    /// Processing and returns them, so concurrent runs never claim the same row twice.
    /// </summary>
    Task<IReadOnlyList<StaleMedia>> ClaimStaleAsync(DateTime cutoff, CancellationToken ct = default);
}
