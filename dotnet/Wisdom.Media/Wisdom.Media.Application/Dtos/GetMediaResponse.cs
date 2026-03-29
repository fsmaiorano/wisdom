namespace Wisdom.Media.Application.Dtos;

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
