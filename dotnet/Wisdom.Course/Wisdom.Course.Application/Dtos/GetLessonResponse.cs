namespace Wisdom.Course.Application.Dtos;

public record GetLessonResponse(
    Guid LessonId,
    Guid CourseId,
    string Title,
    string Description,
    int Position,
    bool IsPreview,
    DateTime? CreatedAt,
    Guid? MediaId = null);
