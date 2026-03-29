using Wisdom.Bff.Clients;

namespace Wisdom.Bff.Aggregations.Courses;

public sealed class CoursePageHandler(
    ICourseApiClient courseClient,
    IOrderApiClient orderClient,
    ILearningApiClient learningClient)
{
    public async Task<CoursePageResponse?> ExecuteAsync(
        Guid courseId, bool isAuthenticated, CancellationToken ct)
    {
        var courseTask = courseClient.GetCourseAsync(courseId, ct);
        var lessonsTask = courseClient.GetLessonsAsync(courseId, ct);
        await Task.WhenAll(courseTask, lessonsTask);

        var course = courseTask.Result;
        var lessons = lessonsTask.Result;

        if (course is null)
            return null;

        EnrollmentInfo enrollment = new(false, null);
        Dictionary<Guid, BackendProgress> progressMap = [];

        if (isAuthenticated)
        {
            var enrollmentResult = await orderClient.CheckEnrollmentAsync(courseId, ct);
            enrollment = new EnrollmentInfo(enrollmentResult?.IsEnrolled ?? false, null);

            if (enrollment.IsEnrolled && lessons.Count > 0)
            {
                var lessonIds = lessons.Select(l => l.LessonId).ToList();
                var progressData = await learningClient.GetCourseProgressAsync(lessonIds, ct);
                progressMap = progressData.ToDictionary(p => p.LessonId);
            }
        }

        var lessonsWithProgress = lessons
            .OrderBy(l => l.Position)
            .Select(l =>
            {
                progressMap.TryGetValue(l.LessonId, out var p);
                return new LessonWithProgress(
                    l.LessonId, l.CourseId, l.Title, l.Description,
                    l.Position, l.IsPreview, l.CreatedAt, l.MediaId,
                    p is null ? null : new LessonProgress(p.WatchedSeconds, p.IsCompleted));
            })
            .ToList();

        var completedCount = lessonsWithProgress.Count(l => l.Progress?.IsCompleted == true);
        var totalCount = lessonsWithProgress.Count;
        var percentage = totalCount > 0 ? Math.Round((double)completedCount / totalCount * 100, 1) : 0;

        return new CoursePageResponse(
            course.CourseId, course.TeacherId, course.Title, course.Description,
            course.Price, course.IsPublished, course.CreatedAt,
            enrollment, lessonsWithProgress,
            new CourseProgressSummary(completedCount, totalCount, percentage));
    }
}
