namespace Wisdom.Learning.Application.Dtos;

public record GetCourseProgressRequest(IReadOnlyList<Guid> LessonIds);
