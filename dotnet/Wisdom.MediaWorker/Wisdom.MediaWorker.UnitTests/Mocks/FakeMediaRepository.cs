using Wisdom.MediaWorker.Application.Abstractions;
namespace Wisdom.MediaWorker.UnitTests.Mocks;

public class FakeMediaRepository : IMediaRepository
{
    public List<(Guid MediaId, string Status)> StatusChanges { get; } = [];
    public (Guid MediaId, string Thumbnail, int Duration)? ReadyCall { get; private set; }
    public List<StaleMedia> StaleToReturn { get; } = [];
    public DateTime? ClaimCutoff { get; private set; }
    public bool ThrowOnSetProcessing { get; set; }

    public string? LastStatus => StatusChanges.Count == 0 ? null : StatusChanges[^1].Status;

    public Task SetStatusAsync(Guid mediaId, string status, CancellationToken ct = default)
    {
        if (ThrowOnSetProcessing && status == "Processing")
            throw new InvalidOperationException("db down");

        StatusChanges.Add((mediaId, status));
        return Task.CompletedTask;
    }

    public Task MarkReadyAsync(Guid mediaId, string thumbnailPath, int durationSeconds, CancellationToken ct = default)
    {
        ReadyCall = (mediaId, thumbnailPath, durationSeconds);
        StatusChanges.Add((mediaId, "Ready"));
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<StaleMedia>> ClaimStaleAsync(DateTime cutoff, CancellationToken ct = default)
    {
        ClaimCutoff = cutoff;
        return Task.FromResult<IReadOnlyList<StaleMedia>>(StaleToReturn.ToList());
    }
}
