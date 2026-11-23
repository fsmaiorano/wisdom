using Wisdom.Streaming.Application.Abstractions;
namespace Wisdom.Streaming.Application.Services;

public enum StreamLookupStatus
{
    Found,
    InvalidQuality,
    MediaNotFound,
    MediaNotReady,
    FileNotFound
}

public sealed record StreamLookup(
    StreamLookupStatus Status,
    string ObjectPath = "",
    long Size = 0,
    string? MediaStatus = null)
{
    public bool IsFound => Status == StreamLookupStatus.Found;
}

public class StreamingService(IMediaStatusReader statusReader, IVideoStorage storage)
{
    public const string DefaultQuality = "720p";
    private static readonly string[] Qualities = ["480p", "720p", "1080p"];

    public async Task<StreamLookup> ResolveAsync(Guid mediaId, string? quality, CancellationToken ct = default)
    {
        quality = string.IsNullOrEmpty(quality) ? DefaultQuality : quality;
        if (!Qualities.Contains(quality))
            return new StreamLookup(StreamLookupStatus.InvalidQuality);

        var status = await statusReader.GetStatusAsync(mediaId, ct);
        if (status is null)
            return new StreamLookup(StreamLookupStatus.MediaNotFound);

        if (status != "Ready")
            return new StreamLookup(StreamLookupStatus.MediaNotReady, MediaStatus: status);

        var objectPath = $"media/{mediaId}/{quality}.mp4";
        var size = await storage.GetSizeAsync(objectPath, ct);
        if (size is null)
            return new StreamLookup(StreamLookupStatus.FileNotFound);

        return new StreamLookup(StreamLookupStatus.Found, objectPath, size.Value);
    }
}
