using System.Diagnostics;
using System.Globalization;
using Wisdom.MediaWorker.Application.Abstractions;
namespace Wisdom.MediaWorker.Infrastructure.Video;

/// <summary>Produces a thumbnail and 480p/720p/1080p renditions using the ffmpeg/ffprobe binaries.</summary>
public class FfmpegVideoTranscoder : IVideoTranscoder
{
    private static readonly (string Label, string Height)[] Resolutions =
    [
        ("480p", "480"),
        ("720p", "720"),
        ("1080p", "1080")
    ];

    public async Task<ProcessedVideo> ProcessAsync(string inputPath, CancellationToken ct = default)
    {
        var dir = Directory.CreateTempSubdirectory("wisdom-video-").FullName;

        try
        {
            var duration = await GetDurationAsync(inputPath, ct);

            var thumbnailPath = Path.Combine(dir, "thumbnail.jpg");
            await RunAsync("ffmpeg",
                ["-y", "-ss", ThumbnailTimestamp(duration), "-i", inputPath, "-frames:v", "1", "-q:v", "2", thumbnailPath],
                ct);

            var videos = new Dictionary<string, string>(Resolutions.Length);
            foreach (var (label, height) in Resolutions)
            {
                var output = Path.Combine(dir, $"{label}.mp4");
                await RunAsync("ffmpeg",
                [
                    "-y", "-i", inputPath,
                    "-vf", $"scale=-2:{height}",
                    "-c:v", "libx264", "-crf", "23", "-preset", "fast",
                    "-c:a", "aac", "-b:a", "128k",
                    output
                ], ct);
                videos[label] = output;
            }

            return new ProcessedVideo(thumbnailPath, videos, duration, dir);
        }
        catch
        {
            Directory.Delete(dir, recursive: true);
            throw;
        }
    }

    public static string ThumbnailTimestamp(int durationSeconds)
        => durationSeconds < 10 ? "00:00:01" : "00:00:05";

    private static async Task<int> GetDurationAsync(string inputPath, CancellationToken ct)
    {
        var output = await RunAsync("ffprobe",
            ["-v", "error", "-show_entries", "format=duration", "-of", "default=noprint_wrappers=1:nokey=1", inputPath],
            ct);

        if (!double.TryParse(output.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds))
            throw new InvalidOperationException($"Could not parse ffprobe duration '{output.Trim()}'.");

        return (int)seconds;
    }

    private static async Task<string> RunAsync(string fileName, string[] args, CancellationToken ct)
    {
        var startInfo = new ProcessStartInfo(fileName)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        foreach (var arg in args)
            startInfo.ArgumentList.Add(arg);

        using var process = Process.Start(startInfo)
                            ?? throw new InvalidOperationException($"Could not start {fileName}.");

        var stdout = process.StandardOutput.ReadToEndAsync(ct);
        var stderr = process.StandardError.ReadToEndAsync(ct);

        try
        {
            await process.WaitForExitAsync(ct);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            throw;
        }

        if (process.ExitCode != 0)
            throw new InvalidOperationException($"{fileName} exited with code {process.ExitCode}: {await stderr}");

        return await stdout;
    }
}
