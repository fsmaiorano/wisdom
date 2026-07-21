namespace Wisdom.Bff.Aggregations.Courses;

public record CoursePageResponse(
    Guid CourseId, Guid TeacherId, string Title, string Description,
    decimal Price, bool IsPublished, DateTime? CreatedAt,
    EnrollmentInfo Enrollment, IReadOnlyList<LessonWithProgress> Lessons,
    CourseProgressSummary Progress);

public record EnrollmentInfo(bool IsEnrolled, DateTime? EnrolledAt);

public record LessonWithProgress(Guid LessonId, Guid CourseId, string Title, string Description,
    int Position, bool IsPreview, DateTime? CreatedAt, Guid? MediaId, LessonProgress? Progress);

public record LessonProgress(int WatchedSeconds, bool IsCompleted);

public record CourseProgressSummary(int CompletedLessons, int TotalLessons, double Percentage);
