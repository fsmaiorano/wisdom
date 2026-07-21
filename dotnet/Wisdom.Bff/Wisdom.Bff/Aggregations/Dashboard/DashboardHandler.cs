using Wisdom.Bff.Clients;

namespace Wisdom.Bff.Aggregations.Dashboard;

public sealed class DashboardHandler(
    IOrderApiClient orderClient,
    ICourseApiClient courseClient,
    ILessonApiClient lessonClient,
    ILearningApiClient learningClient)
{
    public async Task<DashboardResponse> ExecuteAsync(CancellationToken ct)
    {
        var enrollments = await orderClient.GetEnrollmentsAsync(ct);

        if (enrollments.Count == 0)
            return new DashboardResponse([]);

        var courseTasks = enrollments.ToDictionary(e => e.CourseId, e => courseClient.GetCourseAsync(e.CourseId, ct));
        var lessonTasks = enrollments.ToDictionary(e => e.CourseId, e => lessonClient.GetLessonsAsync(e.CourseId, ct));

        await Task.WhenAll(Task.WhenAll(courseTasks.Values), Task.WhenAll(lessonTasks.Values));

        // One batched progress call for all lessons across all enrolled courses.
        var allLessonIds = lessonTasks.Values
            .SelectMany(t => t.Result)
            .Select(l => l.LessonId)
            .ToList();

        var progress = await learningClient.GetCourseProgressAsync(allLessonIds, ct);
        var progressMap = progress.ToDictionary(p => p.LessonId, p => p);

        var enrolledCourses = enrollments
            .Select(enrollment =>
            {
                var course = courseTasks[enrollment.CourseId].Result;
                if (course is null)
                    return null;

                var lessons = lessonTasks[enrollment.CourseId].Result;
                var completedCount = lessons.Count(l => progressMap.TryGetValue(l.LessonId, out var p) && p.IsCompleted);
                var totalCount = lessons.Count;
                var percentage = totalCount > 0 ? Math.Round((double)completedCount / totalCount * 100, 1) : 0;

                return new EnrolledCourseItem(
                    course.CourseId, course.Title, course.Description, course.Price, enrollment.CreatedAt,
                    new CourseProgressSummary(completedCount, totalCount, percentage));
            })
            .Where(item => item is not null)
            .Select(item => item!)
            .ToList();

        return new DashboardResponse(enrolledCourses);
    }
}
