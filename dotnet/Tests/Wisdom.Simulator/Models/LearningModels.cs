namespace Wisdom.Simulator.Models;

public record UpdateProgressRequest(Guid LessonId, int WatchedSeconds, bool IsCompleted);
public record GetProgressResponse(
    Guid ProgressId,
    Guid UserId,
    Guid LessonId,
    int WatchedSeconds,
    bool IsCompleted,
    DateTime? UpdatedAt);

public record GetCourseProgressRequest(IReadOnlyList<Guid> LessonIds);
