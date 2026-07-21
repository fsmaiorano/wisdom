using Wisdom.Bff.Clients;

namespace Wisdom.Bff.Aggregations.Courses;

public sealed class CoursePageHandler(
    ICourseApiClient courseClient,
    ILessonApiClient lessonClient,
    IOrderApiClient orderClient,
    ILearningApiClient learningClient)
{
    public async Task<CoursePageResponse?> ExecuteAsync(Guid courseId, bool isAuthenticated, CancellationToken ct)
    {
        var courseTask = courseClient.GetCourseAsync(courseId, ct);
        var lessonsTask = lessonClient.GetLessonsAsync(courseId, ct);
        await Task.WhenAll(courseTask, lessonsTask);

        var course = courseTask.Result;
        var lessons = lessonsTask.Result;

        if (course is null)
            return null;

        var enrollment = new EnrollmentInfo(false, null);
        var progressMap = new Dictionary<Guid, LessonProgress>();

        if (isAuthenticated)
        {
            var enrollmentResult = await orderClient.CheckEnrollmentAsync(courseId, ct);
            enrollment = new EnrollmentInfo(enrollmentResult.IsEnrolled, null);

            if (enrollment.IsEnrolled && lessons.Count > 0)
            {
                var lessonIds = lessons.Select(l => l.LessonId).ToList();
                var progress = await learningClient.GetCourseProgressAsync(lessonIds, ct);
                progressMap = progress.ToDictionary(p => p.LessonId, p => new LessonProgress(p.WatchedSeconds, p.IsCompleted));
            }
        }

        var lessonsWithProgress = lessons
            .OrderBy(l => l.Position)
            .Select(l => new LessonWithProgress(
                l.LessonId, l.CourseId, l.Title, l.Description, l.Position, l.IsPreview, l.CreatedAt, l.MediaId,
                progressMap.TryGetValue(l.LessonId, out var p) ? p : null))
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
