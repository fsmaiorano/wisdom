namespace Wisdom.Learning.Application.Dtos;

public record GetProgressResponse(
    Guid ProgressId,
    Guid UserId,
    Guid LessonId,
    int WatchedSeconds,
    bool IsCompleted,
    DateTime? UpdatedAt);
