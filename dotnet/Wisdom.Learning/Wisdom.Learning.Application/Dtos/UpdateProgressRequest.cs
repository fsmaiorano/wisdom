namespace Wisdom.Learning.Application.Dtos;

public record UpdateProgressRequest(Guid LessonId, int WatchedSeconds, bool IsCompleted);
