using Wisdom.Bff.Aggregations.Courses;
using Wisdom.Bff.Clients;
using Wisdom.Bff.UnitTests.Mocks;

namespace Wisdom.Bff.UnitTests.Aggregations;

public class CoursePageHandlerTests
{
    private readonly FakeCourseApiClient _courseClient = new();
    private readonly FakeLessonApiClient _lessonClient = new();
    private readonly FakeOrderApiClient _orderClient = new();
    private readonly FakeLearningApiClient _learningClient = new();

    private CoursePageHandler CreateHandler() =>
        new(_courseClient, _lessonClient, _orderClient, _learningClient);

    private static BackendCourse MakeCourse(Guid? id = null) => new(
        id ?? Guid.NewGuid(), Guid.NewGuid(), "Test Course", "Description", 99.99m, true, DateTime.UtcNow);

    private static BackendLesson MakeLesson(Guid courseId, int position = 1) => new(
        Guid.NewGuid(), courseId, $"Lesson {position}", "Desc", position, false, DateTime.UtcNow, null);

    // ── Not found ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenCourseNotFound_ReturnsNull()
    {
        _courseClient.Setup(course: null);

        var result = await CreateHandler().ExecuteAsync(Guid.NewGuid(), false, default);

        Assert.Null(result);
    }

    // ── Anonymous user ─────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenAnonymous_DoesNotCallOrderOrLearning()
    {
        _courseClient.Setup(MakeCourse());

        await CreateHandler().ExecuteAsync(Guid.NewGuid(), isAuthenticated: false, default);

        Assert.Equal(0, _orderClient.CheckEnrollmentCallCount);
        Assert.Equal(0, _learningClient.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAnonymous_EnrollmentIsFalseWithNoProgress()
    {
        var courseId = Guid.NewGuid();
        _courseClient.Setup(MakeCourse(courseId));
        _lessonClient.Setup(courseId, [MakeLesson(courseId)]);

        var result = await CreateHandler().ExecuteAsync(courseId, isAuthenticated: false, default);

        Assert.NotNull(result);
        Assert.False(result.Enrollment.IsEnrolled);
        Assert.All(result.Lessons, l => Assert.Null(l.Progress));
    }

    // ── Authenticated, not enrolled ────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenAuthenticatedAndNotEnrolled_DoesNotCallLearning()
    {
        _courseClient.Setup(MakeCourse());
        _orderClient.EnrollmentResponse = new BackendCheckEnrollment(false);

        await CreateHandler().ExecuteAsync(Guid.NewGuid(), isAuthenticated: true, default);

        Assert.Equal(0, _learningClient.CallCount);
    }

    // ── Authenticated and enrolled ─────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenEnrolled_CallsLearningWithAllLessonIds()
    {
        var courseId = Guid.NewGuid();
        var lesson1 = MakeLesson(courseId, 1);
        var lesson2 = MakeLesson(courseId, 2);
        _courseClient.Setup(MakeCourse(courseId));
        _lessonClient.Setup(courseId, [lesson1, lesson2]);
        _orderClient.EnrollmentResponse = new BackendCheckEnrollment(true);

        await CreateHandler().ExecuteAsync(courseId, isAuthenticated: true, default);

        Assert.Equal(1, _learningClient.CallCount);
        Assert.NotNull(_learningClient.LastLessonIds);
        Assert.Contains(lesson1.LessonId, _learningClient.LastLessonIds);
        Assert.Contains(lesson2.LessonId, _learningClient.LastLessonIds);
    }

    [Fact]
    public async Task ExecuteAsync_MapsProgressToCorrectLesson()
    {
        var courseId = Guid.NewGuid();
        var lesson = MakeLesson(courseId, 1);
        _courseClient.Setup(MakeCourse(courseId));
        _lessonClient.Setup(courseId, [lesson]);
        _orderClient.EnrollmentResponse = new BackendCheckEnrollment(true);
        _learningClient.ProgressResponse =
        [
            new(Guid.NewGuid(), Guid.NewGuid(), lesson.LessonId, 120, true, DateTime.UtcNow)
        ];

        var result = await CreateHandler().ExecuteAsync(courseId, true, default);

        Assert.NotNull(result);
        var lessonResult = Assert.Single(result.Lessons);
        Assert.NotNull(lessonResult.Progress);
        Assert.True(lessonResult.Progress.IsCompleted);
        Assert.Equal(120, lessonResult.Progress.WatchedSeconds);
    }

    // ── Ordering ───────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_LessonsAreOrderedByPosition()
    {
        var courseId = Guid.NewGuid();
        _courseClient.Setup(MakeCourse(courseId));
        _lessonClient.Setup(courseId,
        [
            MakeLesson(courseId, 3),
            MakeLesson(courseId, 1),
            MakeLesson(courseId, 2),
        ]);

        var result = await CreateHandler().ExecuteAsync(courseId, false, default);

        Assert.NotNull(result);
        Assert.Equal([1, 2, 3], result.Lessons.Select(l => l.Position).ToArray());
    }

    // ── Progress summary ───────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_CalculatesProgressPercentageCorrectly()
    {
        var courseId = Guid.NewGuid();
        var lessons = Enumerable.Range(1, 4).Select(i => MakeLesson(courseId, i)).ToList();
        _courseClient.Setup(MakeCourse(courseId));
        _lessonClient.Setup(courseId, lessons);
        _orderClient.EnrollmentResponse = new BackendCheckEnrollment(true);
        // 2 of 4 completed → 50 %
        _learningClient.ProgressResponse =
        [
            new(Guid.NewGuid(), Guid.NewGuid(), lessons[0].LessonId, 300, true, DateTime.UtcNow),
            new(Guid.NewGuid(), Guid.NewGuid(), lessons[1].LessonId, 150, true, DateTime.UtcNow),
        ];

        var result = await CreateHandler().ExecuteAsync(courseId, true, default);

        Assert.NotNull(result);
        Assert.Equal(2, result.Progress.CompletedLessons);
        Assert.Equal(4, result.Progress.TotalLessons);
        Assert.Equal(50.0, result.Progress.Percentage);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoLessons_ProgressPercentageIsZero()
    {
        _courseClient.Setup(MakeCourse());
        _lessonClient.Setup([]);

        var result = await CreateHandler().ExecuteAsync(Guid.NewGuid(), false, default);

        Assert.NotNull(result);
        Assert.Equal(0, result.Progress.Percentage);
        Assert.Equal(0, result.Progress.TotalLessons);
    }
}
