namespace Wisdom.Bff.Clients;

// Mirrors Wisdom.Course.Application.Dtos
public record BackendCourse(
    Guid CourseId,
    Guid TeacherId,
    string Title,
    string Description,
    decimal Price,
    bool IsPublished,
    DateTime? CreatedAt);

public record BackendLesson(
    Guid LessonId,
    Guid CourseId,
    string Title,
    string Description,
    int Position,
    bool IsPreview,
    DateTime? CreatedAt,
    Guid? MediaId);

// Mirrors Wisdom.Order.Application.Dtos
public record BackendCheckEnrollment(bool IsEnrolled);

public record BackendEnrollment(
    Guid EnrollmentId,
    Guid UserId,
    Guid CourseId,
    DateTime? CreatedAt);

// Mirrors Wisdom.Learning.Application.Dtos
public record BackendProgress(
    Guid ProgressId,
    Guid UserId,
    Guid LessonId,
    int WatchedSeconds,
    bool IsCompleted,
    DateTime? UpdatedAt);
