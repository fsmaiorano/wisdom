namespace Wisdom.MediaWorker.Application.Abstractions;

/// <summary>Result of transcoding. Disposing removes the temporary files.</summary>
public sealed class ProcessedVideo(
    string thumbnailPath,
    IReadOnlyDictionary<string, string> videos,
    int durationSeconds,
    string tempDirectory) : IDisposable
{
    public string ThumbnailPath { get; } = thumbnailPath;

    /// <summary>Quality label (e.g. "720p") to local file path.</summary>
    public IReadOnlyDictionary<string, string> Videos { get; } = videos;

    public int DurationSeconds { get; } = durationSeconds;

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(tempDirectory))
                Directory.Delete(tempDirectory, recursive: true);
        }
        catch (IOException)
        {
        }
    }
}

public interface IVideoTranscoder
{
    Task<ProcessedVideo> ProcessAsync(string inputPath, CancellationToken ct = default);
}
