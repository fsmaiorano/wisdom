namespace Wisdom.Bff.Aggregations.Dashboard;

public record DashboardResponse(IReadOnlyList<EnrolledCourseItem> EnrolledCourses);

public record EnrolledCourseItem(
    Guid CourseId,
    string Title,
    string Description,
    decimal Price,
    DateTime? EnrolledAt,
    CourseProgressSummary Progress);

public record CourseProgressSummary(int CompletedLessons, int TotalLessons, double Percentage);
