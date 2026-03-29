using System.Net;
using System.Net.Http.Json;
using Wisdom.Tests.Common.Auth;
using Wisdom.Tests.Common.Builders;
using Wisdom.Learning.Application.Dtos;
using Wisdom.Learning.IntegrationTests.Fixtures;
namespace Wisdom.Learning.IntegrationTests.Progress;

public class ProgressTests(LearningApiFixture fixture) : IClassFixture<LearningApiFixture>
{
    private static string NewUserToken() => AuthHelper.TokenForEmail(BogusFactory.Email());

    [Fact]
    public async Task UpdateProgress_WithValidRequest_Returns200WithProgressData()
    {
        var token = NewUserToken();
        var lessonId = BogusFactory.NewGuid();
        var watchedSeconds = BogusFactory.Faker.Random.Int(1, 3600);
        var request = new UpdateProgressRequest(lessonId, watchedSeconds, false);

        var response = await fixture.DoPost("/api/progress/", request, token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<GetProgressResponse>();
        Assert.NotNull(body);
        Assert.Equal(lessonId, body.LessonId);
        Assert.Equal(watchedSeconds, body.WatchedSeconds);
        Assert.False(body.IsCompleted);
    }

    [Fact]
    public async Task UpdateProgress_WithoutAuth_Returns401()
    {
        var request = new UpdateProgressRequest(BogusFactory.NewGuid(), 60, false);

        var response = await fixture.DoPost("/api/progress/", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProgress_ThenGetByLessonId_ReturnsUpdatedProgress()
    {
        var token = NewUserToken();
        var lessonId = BogusFactory.NewGuid();
        var watchedSeconds = BogusFactory.Faker.Random.Int(60, 600);

        await fixture.DoPost("/api/progress/", new UpdateProgressRequest(lessonId, watchedSeconds, false), token);

        var response = await fixture.DoGet($"/api/progress/lesson/{lessonId}", token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<GetProgressResponse>();
        Assert.NotNull(body);
        Assert.Equal(lessonId, body.LessonId);
        Assert.Equal(watchedSeconds, body.WatchedSeconds);
    }

    [Fact]
    public async Task UpdateProgress_ToCompleted_IsCompletedTrue()
    {
        var token = NewUserToken();
        var lessonId = BogusFactory.NewGuid();

        await fixture.DoPost("/api/progress/", new UpdateProgressRequest(lessonId, 300, false), token);

        var completeResponse = await fixture.DoPost(
            "/api/progress/", new UpdateProgressRequest(lessonId, 600, true), token);

        Assert.Equal(HttpStatusCode.OK, completeResponse.StatusCode);

        var body = await completeResponse.Content.ReadFromJsonAsync<GetProgressResponse>();
        Assert.NotNull(body);
        Assert.True(body.IsCompleted);
        Assert.Equal(600, body.WatchedSeconds);
    }

    [Fact]
    public async Task GetCourseProgress_WithMultipleLessons_ReturnsAllProgress()
    {
        var token = NewUserToken();
        var lessonId1 = BogusFactory.NewGuid();
        var lessonId2 = BogusFactory.NewGuid();

        await fixture.DoPost("/api/progress/", new UpdateProgressRequest(lessonId1, 120, false), token);
        await fixture.DoPost("/api/progress/", new UpdateProgressRequest(lessonId2, 600, true), token);

        var request = new GetCourseProgressRequest([lessonId1, lessonId2]);
        var response = await fixture.DoPost("/api/progress/course", request, token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<List<GetProgressResponse>>();
        Assert.NotNull(body);
        Assert.Equal(2, body.Count);
        Assert.Contains(body, p => p.LessonId == lessonId1 && !p.IsCompleted);
        Assert.Contains(body, p => p.LessonId == lessonId2 && p.IsCompleted);
    }
}
