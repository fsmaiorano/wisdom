namespace Wisdom.Simulator.Models;

public record CreateCourseRequest(string Title, string Description, decimal Price);
public record CreateCourseResponse(
    Guid CourseId,
    Guid TeacherId,
    string Title,
    string Description,
    decimal Price,
    bool IsPublished,
    DateTime? CreatedAt);

public record CreateLessonRequest(string Title, string Description, int Position, bool IsPreview);
public record GetLessonResponse(
    Guid LessonId,
    Guid CourseId,
    string Title,
    string Description,
    int Position,
    bool IsPreview,
    DateTime? CreatedAt,
    Guid? MediaId = null);

public record GetCourseResponse(
    Guid CourseId,
    Guid TeacherId,
    string Title,
    string Description,
    decimal Price,
    bool IsPublished,
    DateTime? CreatedAt);

public record PublishCourseRequest(bool Publish);
