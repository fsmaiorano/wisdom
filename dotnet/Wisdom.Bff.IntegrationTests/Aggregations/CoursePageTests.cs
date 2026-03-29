using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Wisdom.Bff.Aggregations.Courses;
using Wisdom.Bff.IntegrationTests.Fixtures;
using Wisdom.Tests.Common.Auth;
using Wisdom.Tests.Common.Builders;

namespace Wisdom.Bff.IntegrationTests.Aggregations;

public class CoursePageTests(BffApiFixture fixture) : IClassFixture<BffApiFixture>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private record StubCourse(Guid CourseId, Guid TeacherId, string Title, string Description,
        decimal Price, bool IsPublished, DateTime? CreatedAt);
    private record StubLesson(Guid LessonId, Guid CourseId, string Title, string Description,
        int Position, bool IsPreview, DateTime? CreatedAt, Guid? MediaId);
    private record StubCheckEnrollment(bool IsEnrolled);
    private record StubProgress(Guid ProgressId, Guid UserId, Guid LessonId,
        int WatchedSeconds, bool IsCompleted, DateTime? UpdatedAt);

    [Fact]
    public async Task GetCoursePage_WhenCourseNotFound_Returns404()
    {
        var courseId = BogusFactory.NewGuid();
        fixture.CourseBackend.SetupNotFound($"/api/courses/{courseId}");

        var response = await fixture.DoGet($"/bff/courses/{courseId}/page");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetCoursePage_WhenAnonymous_ReturnsCourseWithoutEnrollmentOrProgress()
    {
        var courseId = BogusFactory.NewGuid();
        var lessonId = BogusFactory.NewGuid();
        var title = BogusFactory.Faker.Lorem.Sentence(3);

        fixture.CourseBackend.SetupGet($"/api/courses/{courseId}",
            new StubCourse(courseId, BogusFactory.NewGuid(), title, BogusFactory.Faker.Lorem.Paragraph(),
                BogusFactory.Faker.Random.Decimal(10, 500), true, null));
        fixture.CourseBackend.SetupGet($"/api/courses/{courseId}/lessons",
            new[] { new StubLesson(lessonId, courseId, BogusFactory.Faker.Lorem.Sentence(2),
                BogusFactory.Faker.Lorem.Sentence(5), 1, true, null, null) });

        var response = await fixture.DoGet($"/bff/courses/{courseId}/page");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<CoursePageResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(courseId, body.CourseId);
        Assert.Equal(title, body.Title);
        Assert.Single(body.Lessons);
        Assert.False(body.Enrollment.IsEnrolled);
        Assert.All(body.Lessons, l => Assert.Null(l.Progress));
    }

    [Fact]
    public async Task GetCoursePage_WhenAuthenticatedAndEnrolled_ReturnsProgressData()
    {
        var courseId = BogusFactory.NewGuid();
        var lessonId = BogusFactory.NewGuid();

        fixture.CourseBackend.SetupGet($"/api/courses/{courseId}",
            new StubCourse(courseId, BogusFactory.NewGuid(), BogusFactory.Faker.Lorem.Sentence(3),
                BogusFactory.Faker.Lorem.Paragraph(), BogusFactory.Faker.Random.Decimal(10, 500), true, null));
        fixture.CourseBackend.SetupGet($"/api/courses/{courseId}/lessons",
            new[] { new StubLesson(lessonId, courseId, BogusFactory.Faker.Lorem.Sentence(2),
                BogusFactory.Faker.Lorem.Sentence(5), 1, false, null, null) });

        fixture.OrderBackend.SetupGet($"/api/orders/enrollments/{courseId}/check",
            new StubCheckEnrollment(true));

        fixture.LearningBackend.SetupPost("/api/progress/course",
            new[] { new StubProgress(BogusFactory.NewGuid(), BogusFactory.NewGuid(), lessonId, 300, true, null) });

        var response = await fixture.DoGet($"/bff/courses/{courseId}/page",
            AuthHelper.TokenForEmail(BogusFactory.Email()));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<CoursePageResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.True(body.Enrollment.IsEnrolled);
        var lesson = Assert.Single(body.Lessons);
        Assert.NotNull(lesson.Progress);
        Assert.True(lesson.Progress.IsCompleted);
        Assert.Equal(1, body.Progress.CompletedLessons);
        Assert.Equal(100.0, body.Progress.Percentage);
    }

    [Fact]
    public async Task GetCoursePage_WhenAuthenticatedButNotEnrolled_ReturnsNoProgress()
    {
        var courseId = BogusFactory.NewGuid();
        var lessonId = BogusFactory.NewGuid();

        fixture.CourseBackend.SetupGet($"/api/courses/{courseId}",
            new StubCourse(courseId, BogusFactory.NewGuid(), BogusFactory.Faker.Lorem.Sentence(3),
                BogusFactory.Faker.Lorem.Paragraph(), BogusFactory.Faker.Random.Decimal(10, 500), true, null));
        fixture.CourseBackend.SetupGet($"/api/courses/{courseId}/lessons",
            new[] { new StubLesson(lessonId, courseId, BogusFactory.Faker.Lorem.Sentence(2),
                BogusFactory.Faker.Lorem.Sentence(5), 1, false, null, null) });

        fixture.OrderBackend.SetupGet($"/api/orders/enrollments/{courseId}/check",
            new StubCheckEnrollment(false));

        var response = await fixture.DoGet($"/bff/courses/{courseId}/page",
            AuthHelper.TokenForEmail(BogusFactory.Email()));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<CoursePageResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.False(body.Enrollment.IsEnrolled);
        Assert.All(body.Lessons, l => Assert.Null(l.Progress));
    }
}
