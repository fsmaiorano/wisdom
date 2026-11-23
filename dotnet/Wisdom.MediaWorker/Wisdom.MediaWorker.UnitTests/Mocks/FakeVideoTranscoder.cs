using Wisdom.MediaWorker.Application.Abstractions;
namespace Wisdom.MediaWorker.UnitTests.Mocks;

public class FakeVideoTranscoder : IVideoTranscoder
{
    public int DurationSeconds { get; set; } = 42;
    public bool Fail { get; set; }

    public Task<ProcessedVideo> ProcessAsync(string inputPath, CancellationToken ct = default)
    {
        if (Fail)
            throw new InvalidOperationException("ffmpeg failed");

        var dir = Directory.CreateTempSubdirectory("wisdom-test-").FullName;
        var thumbnail = Path.Combine(dir, "thumbnail.jpg");
        File.WriteAllText(thumbnail, "thumb");

        var videos = new Dictionary<string, string>();
        foreach (var label in new[] { "480p", "720p", "1080p" })
        {
            var path = Path.Combine(dir, $"{label}.mp4");
            File.WriteAllText(path, label);
            videos[label] = path;
        }

        return Task.FromResult(new ProcessedVideo(thumbnail, videos, DurationSeconds, dir));
    }
}
