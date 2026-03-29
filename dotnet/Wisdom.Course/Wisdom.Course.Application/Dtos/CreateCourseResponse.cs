namespace Wisdom.Course.Application.Dtos;

public record CreateCourseResponse(
    Guid CourseId,
    Guid TeacherId,
    string Title,
    string Description,
    decimal Price,
    bool IsPublished,
    DateTime? CreatedAt);
