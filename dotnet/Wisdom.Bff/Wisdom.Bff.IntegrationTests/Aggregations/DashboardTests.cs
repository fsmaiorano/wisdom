using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Wisdom.Bff.Aggregations.Dashboard;
using Wisdom.Bff.IntegrationTests.Fixtures;
using Wisdom.Tests.Common.Auth;
using Wisdom.Tests.Common.Builders;

namespace Wisdom.Bff.IntegrationTests.Aggregations;

public class DashboardTests(BffApiFixture fixture) : IClassFixture<BffApiFixture>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private record StubEnrollment(Guid EnrollmentId, Guid UserId, Guid CourseId, DateTime? CreatedAt);
    private record StubCourse(Guid CourseId, Guid TeacherId, string Title, string Description,
        decimal Price, bool IsPublished, DateTime? CreatedAt);
    private record StubLesson(Guid LessonId, Guid CourseId, string Title, string Description,
        int Position, bool IsPreview, DateTime? CreatedAt, Guid? MediaId);
    private record StubProgress(Guid ProgressId, Guid UserId, Guid LessonId,
        int WatchedSeconds, bool IsCompleted, DateTime? UpdatedAt);

    [Fact]
    public async Task GetDashboard_WhenNotAuthenticated_Returns401()
    {
        var response = await fixture.DoGet("/bff/dashboard");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetDashboard_WhenNoEnrollments_ReturnsEmptyList()
    {
        fixture.OrderBackend.SetupGet("/api/orders/enrollments", Array.Empty<StubEnrollment>());

        var response = await fixture.DoGet("/bff/dashboard",
            AuthHelper.TokenForEmail(BogusFactory.Email()));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<DashboardResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Empty(body.EnrolledCourses);
    }

    [Fact]
    public async Task GetDashboard_WhenAuthenticated_ReturnsEnrolledCoursesWithProgress()
    {
        var courseId = BogusFactory.NewGuid();
        var lessonId = BogusFactory.NewGuid();
        var enrollmentId = BogusFactory.NewGuid();
        var userId = BogusFactory.NewGuid();
        var courseTitle = BogusFactory.Faker.Lorem.Sentence(3);

        fixture.OrderBackend.SetupGet("/api/orders/enrollments",
            new[] { new StubEnrollment(enrollmentId, userId, courseId, DateTime.UtcNow) });

        fixture.CourseBackend.SetupGet($"/api/courses/{courseId}",
            new StubCourse(courseId, BogusFactory.NewGuid(), courseTitle,
                BogusFactory.Faker.Lorem.Paragraph(), BogusFactory.Faker.Random.Decimal(10, 500), true, null));
        fixture.CourseBackend.SetupGet($"/api/courses/{courseId}/lessons",
            new[] { new StubLesson(lessonId, courseId, BogusFactory.Faker.Lorem.Sentence(2),
                BogusFactory.Faker.Lorem.Sentence(5), 1, false, null, null) });

        fixture.LearningBackend.SetupPost("/api/progress/course",
            new[] { new StubProgress(BogusFactory.NewGuid(), userId, lessonId, 600, true, null) });

        var response = await fixture.DoGet("/bff/dashboard",
            AuthHelper.TokenForEmail(BogusFactory.Email()));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<DashboardResponse>(JsonOptions);
        Assert.NotNull(body);
        var item = Assert.Single(body.EnrolledCourses);
        Assert.Equal(courseId, item.CourseId);
        Assert.Equal(courseTitle, item.Title);
        Assert.Equal(1, item.Progress.CompletedLessons);
        Assert.Equal(100.0, item.Progress.Percentage);
    }

    [Fact]
    public async Task GetDashboard_WithMultipleEnrollments_ReturnsAllCourses()
    {
        var courseId1 = BogusFactory.NewGuid();
        var courseId2 = BogusFactory.NewGuid();
        var lessonId1 = BogusFactory.NewGuid();
        var lessonId2 = BogusFactory.NewGuid();
        var userId = BogusFactory.NewGuid();

        fixture.OrderBackend.SetupGet("/api/orders/enrollments", new[]
        {
            new StubEnrollment(BogusFactory.NewGuid(), userId, courseId1, DateTime.UtcNow),
            new StubEnrollment(BogusFactory.NewGuid(), userId, courseId2, DateTime.UtcNow)
        });

        fixture.CourseBackend.SetupGet($"/api/courses/{courseId1}",
            new StubCourse(courseId1, BogusFactory.NewGuid(), BogusFactory.Faker.Lorem.Sentence(3),
                BogusFactory.Faker.Lorem.Paragraph(), 49m, true, null));
        fixture.CourseBackend.SetupGet($"/api/courses/{courseId1}/lessons",
            new[] { new StubLesson(lessonId1, courseId1, BogusFactory.Faker.Lorem.Sentence(2),
                BogusFactory.Faker.Lorem.Sentence(5), 1, false, null, null) });

        fixture.CourseBackend.SetupGet($"/api/courses/{courseId2}",
            new StubCourse(courseId2, BogusFactory.NewGuid(), BogusFactory.Faker.Lorem.Sentence(3),
                BogusFactory.Faker.Lorem.Paragraph(), 79m, true, null));
        fixture.CourseBackend.SetupGet($"/api/courses/{courseId2}/lessons",
            new[] { new StubLesson(lessonId2, courseId2, BogusFactory.Faker.Lorem.Sentence(2),
                BogusFactory.Faker.Lorem.Sentence(5), 1, false, null, null) });

        fixture.LearningBackend.SetupPost("/api/progress/course",
            new[] { new StubProgress(BogusFactory.NewGuid(), userId, lessonId1, 300, false, null) });

        var response = await fixture.DoGet("/bff/dashboard",
            AuthHelper.TokenForEmail(BogusFactory.Email()));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<DashboardResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(2, body.EnrolledCourses.Count);
    }
}
