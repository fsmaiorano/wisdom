using System.Net;
using System.Net.Http.Json;
using Wisdom.Course.Application.Dtos;
using Wisdom.Course.IntegrationTests.Fixtures;
using Wisdom.Tests.Common.Auth;
using Wisdom.Tests.Common.Builders;
namespace Wisdom.Course.IntegrationTests.Courses;

public class CourseTests(CourseApiFixture fixture) : IClassFixture<CourseApiFixture>
{
    private static string NewTeacherToken() => AuthHelper.TokenForEmail(BogusFactory.Email());

    private async Task<CreateCourseResponse> CreateCourseAsync(string token)
    {
        var request = new CreateCourseRequest(
            BogusFactory.Faker.Lorem.Sentence(3),
            BogusFactory.Faker.Lorem.Paragraph(),
            BogusFactory.Faker.Random.Decimal(1, 500));
        var response = await fixture.DoPost("/api/courses", request, token);
        return (await response.Content.ReadFromJsonAsync<CreateCourseResponse>())!;
    }

    [Fact]
    public async Task CreateCourse_WithValidRequest_Returns201WithCourseData()
    {
        var token = NewTeacherToken();
        var request = new CreateCourseRequest(
            BogusFactory.Faker.Lorem.Sentence(3),
            BogusFactory.Faker.Lorem.Paragraph(),
            BogusFactory.Faker.Random.Decimal(1, 500));

        var response = await fixture.DoPost("/api/courses", request, token);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<CreateCourseResponse>();
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body.CourseId);
        Assert.Equal(request.Title, body.Title);
        Assert.Equal(request.Description, body.Description);
        Assert.Equal(request.Price, body.Price);
        Assert.False(body.IsPublished);
    }

    [Fact]
    public async Task CreateCourse_WithoutAuth_Returns401()
    {
        var request = new CreateCourseRequest(
            BogusFactory.Faker.Lorem.Sentence(3),
            BogusFactory.Faker.Lorem.Paragraph(),
            BogusFactory.Faker.Random.Decimal(1, 100));

        var response = await fixture.DoPost("/api/courses", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetCourse_WithValidId_Returns200WithCourseData()
    {
        var token = NewTeacherToken();
        var created = await CreateCourseAsync(token);

        var response = await fixture.DoGet($"/api/courses/{created.CourseId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetCourseResponse>();
        Assert.NotNull(body);
        Assert.Equal(created.CourseId, body.CourseId);
        Assert.Equal(created.Title, body.Title);
        Assert.Equal(created.Description, body.Description);
        Assert.Equal(created.Price, body.Price);
    }

    [Fact]
    public async Task GetCourse_WithInvalidId_Returns404()
    {
        var response = await fixture.DoGet($"/api/courses/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCourse_WithValidRequest_Returns200WithUpdatedData()
    {
        var token = NewTeacherToken();
        var created = await CreateCourseAsync(token);

        var updateRequest = new UpdateCourseRequest(
            BogusFactory.Faker.Lorem.Sentence(3),
            BogusFactory.Faker.Lorem.Paragraph(),
            BogusFactory.Faker.Random.Decimal(1, 999));

        var response = await fixture.DoPut($"/api/courses/{created.CourseId}", updateRequest, token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetCourseResponse>();
        Assert.NotNull(body);
        Assert.Equal(updateRequest.Title, body.Title);
        Assert.Equal(updateRequest.Description, body.Description);
        Assert.Equal(updateRequest.Price, body.Price);
    }

    [Fact]
    public async Task PublishCourse_AfterCreation_Returns200WithIsPublishedTrue()
    {
        var token = NewTeacherToken();
        var created = await CreateCourseAsync(token);
        Assert.False(created.IsPublished);

        var response = await fixture.DoPatch($"/api/courses/{created.CourseId}/publish",
            new { publish = true }, token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetCourseResponse>();
        Assert.NotNull(body);
        Assert.True(body.IsPublished);
    }

    [Fact]
    public async Task ListCourses_Returns200WithList()
    {
        var token = NewTeacherToken();
        await CreateCourseAsync(token);

        var response = await fixture.DoGet("/api/courses");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<List<GetCourseResponse>>();
        Assert.NotNull(body);
        Assert.NotEmpty(body);
    }
}
