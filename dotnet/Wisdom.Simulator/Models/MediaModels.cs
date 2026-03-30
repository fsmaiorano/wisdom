namespace Wisdom.Simulator.Models;

public record UploadMediaRequest(Guid LessonId, string Type);

public record GetMediaResponse(
    Guid MediaId,
    Guid LessonId,
    string Type,
    string Status,
    string FilePath,
    string? ThumbnailPath,
    int? Duration,
    DateTime? CreatedAt,
    string? FileUrl,
    string? ThumbnailUrl);
