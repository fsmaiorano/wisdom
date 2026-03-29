using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Wisdom.Course.Application.Dtos;
using Wisdom.Course.IntegrationTests.Fixtures;
using Wisdom.Tests.Common.Auth;
using Wisdom.Tests.Common.Builders;
namespace Wisdom.Course.IntegrationTests.Lessons;

public class LessonTests(CourseApiFixture fixture) : IClassFixture<CourseApiFixture>
{
    private string NewTeacherToken() => AuthHelper.TokenForEmail(BogusFactory.Email());

    private async Task<Guid> CreateCourseAsync(string token)
    {
        var request = new CreateCourseRequest(
            BogusFactory.Faker.Lorem.Sentence(3),
            BogusFactory.Faker.Lorem.Paragraph(),
            BogusFactory.Faker.Random.Decimal(1, 500));
        var response = await fixture.DoPost("/api/courses", request, token);
        var created = await response.Content.ReadFromJsonAsync<CreateCourseResponse>();
        return created!.CourseId;
    }

    private async Task<HttpResponseMessage> CreateLessonFormAsync(
        Guid courseId, string title, string description, int position, bool isPreview, string token,
        Stream? videoStream = null, string? videoFileName = null)
    {
        var client = fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var form = new MultipartFormDataContent();
        form.Add(new StringContent(title), "title");
        form.Add(new StringContent(description), "description");
        form.Add(new StringContent(position.ToString()), "position");
        form.Add(new StringContent(isPreview.ToString().ToLower()), "isPreview");

        if (videoStream is not null && videoFileName is not null)
        {
            var fileContent = new StreamContent(videoStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("video/x-matroska");
            form.Add(fileContent, "video", videoFileName);
        }

        return await client.PostAsync($"/api/courses/{courseId}/lessons", form);
    }

    [Fact]
    public async Task CreateLesson_WithValidRequest_Returns201()
    {
        var token = NewTeacherToken();
        var courseId = await CreateCourseAsync(token);
        var title = BogusFactory.Faker.Lorem.Sentence(3);
        var description = BogusFactory.Faker.Lorem.Paragraph();

        var response = await CreateLessonFormAsync(courseId, title, description, 1, true, token);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<GetLessonResponse>();
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body.LessonId);
        Assert.Equal(courseId, body.CourseId);
        Assert.Equal(title, body.Title);
        Assert.Equal(1, body.Position);
        Assert.True(body.IsPreview);
        Assert.Null(body.MediaId);
    }

    [Fact]
    public async Task CreateLesson_WithVideo_Returns201WithMediaId()
    {
        var token = NewTeacherToken();
        var courseId = await CreateCourseAsync(token);

        var videoBytes = new byte[] { 0x1A, 0x45, 0xDF, 0xA3 }; // MKV magic bytes
        using var videoStream = new MemoryStream(videoBytes);

        var response = await CreateLessonFormAsync(
            courseId,
            BogusFactory.Faker.Lorem.Sentence(3),
            BogusFactory.Faker.Lorem.Paragraph(),
            1, false, token, videoStream, "lesson.mkv");

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<GetLessonResponse>();
        Assert.NotNull(body);
        Assert.NotNull(body.MediaId);
        Assert.NotEqual(Guid.Empty, body.MediaId!.Value);
    }

    [Fact]
    public async Task UpdateLesson_WithValidRequest_Returns200WithUpdatedData()
    {
        var token = NewTeacherToken();
        var courseId = await CreateCourseAsync(token);

        var createResponse = await CreateLessonFormAsync(
            courseId,
            BogusFactory.Faker.Lorem.Sentence(3),
            BogusFactory.Faker.Lorem.Paragraph(),
            1, true, token);
        var created = await createResponse.Content.ReadFromJsonAsync<GetLessonResponse>();
        Assert.NotNull(created);

        var updateRequest = new UpdateLessonRequest(
            BogusFactory.Faker.Lorem.Sentence(3),
            BogusFactory.Faker.Lorem.Paragraph(),
            2,
            false);

        var response = await fixture.DoPut(
            $"/api/courses/{courseId}/lessons/{created.LessonId}", updateRequest, token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<GetLessonResponse>();
        Assert.NotNull(body);
        Assert.Equal(updateRequest.Title, body.Title);
        Assert.Equal(updateRequest.Description, body.Description);
        Assert.Equal(2, body.Position);
        Assert.False(body.IsPreview);
    }

    [Fact]
    public async Task DeleteLesson_WithValidId_Returns204()
    {
        var token = NewTeacherToken();
        var courseId = await CreateCourseAsync(token);

        var createResponse = await CreateLessonFormAsync(
            courseId,
            BogusFactory.Faker.Lorem.Sentence(3),
            BogusFactory.Faker.Lorem.Paragraph(),
            1, false, token);
        var created = await createResponse.Content.ReadFromJsonAsync<GetLessonResponse>();
        Assert.NotNull(created);

        var response = await fixture.DoDelete(
            $"/api/courses/{courseId}/lessons/{created.LessonId}", token);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task GetLessons_ByCourseId_Returns200WithCreatedLessons()
    {
        var token = NewTeacherToken();
        var courseId = await CreateCourseAsync(token);

        await CreateLessonFormAsync(courseId, BogusFactory.Faker.Lorem.Sentence(3),
            BogusFactory.Faker.Lorem.Paragraph(), 1, true, token);
        await CreateLessonFormAsync(courseId, BogusFactory.Faker.Lorem.Sentence(3),
            BogusFactory.Faker.Lorem.Paragraph(), 2, false, token);

        var response = await fixture.DoGet($"/api/courses/{courseId}/lessons");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<List<GetLessonResponse>>();
        Assert.NotNull(body);
        Assert.Equal(2, body.Count);
    }
}
