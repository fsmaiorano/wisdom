using Wisdom.Bff.Aggregations.Dashboard;
using Wisdom.Bff.Clients;
using Wisdom.Bff.UnitTests.Mocks;

namespace Wisdom.Bff.UnitTests.Aggregations;

public class DashboardHandlerTests
{
    private readonly FakeOrderApiClient _orderClient = new();
    private readonly FakeCourseApiClient _courseClient = new();
    private readonly FakeLearningApiClient _learningClient = new();

    private DashboardHandler CreateHandler() =>
        new(_orderClient, _courseClient, _learningClient);

    private static BackendCourse MakeCourse(Guid courseId) =>
        new(courseId, Guid.NewGuid(), "Course", "Desc", 49.99m, true, DateTime.UtcNow);

    private static BackendLesson MakeLesson(Guid courseId, int position = 1) =>
        new(Guid.NewGuid(), courseId, $"Lesson {position}", "Desc", position, false, null, null);

    private static BackendEnrollment MakeEnrollment(Guid courseId) =>
        new(Guid.NewGuid(), Guid.NewGuid(), courseId, DateTime.UtcNow);

    // ── Empty state ────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenNoEnrollments_ReturnsEmptyList()
    {
        _orderClient.EnrollmentsResponse = [];

        var result = await CreateHandler().ExecuteAsync(default);

        Assert.Empty(result.EnrolledCourses);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoEnrollments_NeverCallsLearning()
    {
        _orderClient.EnrollmentsResponse = [];

        await CreateHandler().ExecuteAsync(default);

        Assert.Equal(0, _learningClient.CallCount);
    }

    // ── Normal flow ────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_ReturnsOneItemPerValidEnrollment()
    {
        var courseId1 = Guid.NewGuid();
        var courseId2 = Guid.NewGuid();
        _orderClient.EnrollmentsResponse = [MakeEnrollment(courseId1), MakeEnrollment(courseId2)];
        _courseClient.Setup(courseId1, MakeCourse(courseId1), []);
        _courseClient.Setup(courseId2, MakeCourse(courseId2), []);

        var result = await CreateHandler().ExecuteAsync(default);

        Assert.Equal(2, result.EnrolledCourses.Count);
    }

    [Fact]
    public async Task ExecuteAsync_MakesOneProgressCallWithAllLessonIds()
    {
        var courseId = Guid.NewGuid();
        var lesson1 = MakeLesson(courseId, 1);
        var lesson2 = MakeLesson(courseId, 2);
        _orderClient.EnrollmentsResponse = [MakeEnrollment(courseId)];
        _courseClient.Setup(courseId, MakeCourse(courseId), [lesson1, lesson2]);

        await CreateHandler().ExecuteAsync(default);

        Assert.Equal(1, _learningClient.CallCount);
        Assert.NotNull(_learningClient.LastLessonIds);
        Assert.Equal(2, _learningClient.LastLessonIds.Count);
        Assert.Contains(lesson1.LessonId, _learningClient.LastLessonIds);
        Assert.Contains(lesson2.LessonId, _learningClient.LastLessonIds);
    }

    [Fact]
    public async Task ExecuteAsync_CalculatesProgressPerCourse()
    {
        var courseId = Guid.NewGuid();
        var lesson = MakeLesson(courseId, 1);
        _orderClient.EnrollmentsResponse = [MakeEnrollment(courseId)];
        _courseClient.Setup(courseId, MakeCourse(courseId), [lesson]);
        _learningClient.ProgressResponse =
        [
            new(Guid.NewGuid(), Guid.NewGuid(), lesson.LessonId, 300, true, DateTime.UtcNow)
        ];

        var result = await CreateHandler().ExecuteAsync(default);

        var item = Assert.Single(result.EnrolledCourses);
        Assert.Equal(1, item.Progress.CompletedLessons);
        Assert.Equal(1, item.Progress.TotalLessons);
        Assert.Equal(100.0, item.Progress.Percentage);
    }

    [Fact]
    public async Task ExecuteAsync_FiltersOutEnrollmentsWhenCourseIsNotFound()
    {
        var courseId = Guid.NewGuid();
        _orderClient.EnrollmentsResponse = [MakeEnrollment(courseId)];
        _courseClient.Setup(courseId, course: null);   // backend returns 404

        var result = await CreateHandler().ExecuteAsync(default);

        Assert.Empty(result.EnrolledCourses);
    }
}
