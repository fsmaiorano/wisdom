using System.Net;
using System.Net.Http.Json;
using Wisdom.Media.Application.Dtos;
using Wisdom.Media.Domain.Entities;
using Wisdom.Media.Domain.Enums;
using Wisdom.Media.IntegrationTests.Fixtures;
using Wisdom.Tests.Common.Auth;
using Wisdom.Tests.Common.Builders;
namespace Wisdom.Media.IntegrationTests.Media;

public class MediaTests(MediaApiFixture fixture) : IClassFixture<MediaApiFixture>
{
    private static string NewUserToken() => AuthHelper.TokenForEmail(BogusFactory.Email());

    private async Task<MediaEntity> SeedMediaAsync(MediaTypeEnum type = MediaTypeEnum.Video)
    {
        var media = MediaEntity.Create(BogusFactory.NewGuid(), type, $"media/test/{BogusFactory.UniqueId()}.mp4");
        fixture.DbContext.Media.Add(media);
        await fixture.DbContext.SaveChangesAsync();
        return media;
    }

    private async Task<MediaEntity> SeedMediaWithThumbnailAsync()
    {
        var filePath = $"media/test/{BogusFactory.UniqueId()}.mp4";
        var thumbnailPath = $"media/test/{BogusFactory.UniqueId()}-thumb.jpg";
        var media = MediaEntity.Create(BogusFactory.NewGuid(), MediaTypeEnum.Video, filePath, thumbnailPath);
        fixture.DbContext.Media.Add(media);
        await fixture.DbContext.SaveChangesAsync();
        return media;
    }

    private static string ExpectedPresignedUrl(string objectName)
        => $"https://test-minio/wisdom-media/{objectName}?expires=3600";

    [Fact]
    public async Task GetMedia_WithInvalidId_Returns404()
    {
        var response = await fixture.DoGet($"/api/media/{BogusFactory.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetMedia_WithValidId_ReturnsAllMediaFields()
    {
        var media = await SeedMediaAsync(MediaTypeEnum.Video);

        var response = await fixture.DoGet($"/api/media/{media.Id.Value}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetMediaResponse>();
        Assert.NotNull(body);
        Assert.Equal(media.Id.Value, body.MediaId);
        Assert.Equal(media.LessonId, body.LessonId);
        Assert.Equal(media.Type.ToString(), body.Type);
        Assert.Equal(media.Status.ToString(), body.Status);
    }

    [Fact]
    public async Task GetMedia_ReturnsPresignedFileUrl()
    {
        var media = await SeedMediaAsync();

        var response = await fixture.DoGet($"/api/media/{media.Id.Value}");

        var body = await response.Content.ReadFromJsonAsync<GetMediaResponse>();
        Assert.NotNull(body!.FileUrl);
        Assert.Equal(ExpectedPresignedUrl(media.FilePath), body.FileUrl);
    }

    [Fact]
    public async Task GetMedia_WithoutThumbnail_ReturnsThumbnailUrlNull()
    {
        var media = await SeedMediaAsync();

        var response = await fixture.DoGet($"/api/media/{media.Id.Value}");

        var body = await response.Content.ReadFromJsonAsync<GetMediaResponse>();
        Assert.Null(body!.ThumbnailUrl);
    }

    [Fact]
    public async Task GetMedia_WithThumbnail_ReturnsThumbnailUrl()
    {
        var media = await SeedMediaWithThumbnailAsync();

        var response = await fixture.DoGet($"/api/media/{media.Id.Value}");

        var body = await response.Content.ReadFromJsonAsync<GetMediaResponse>();
        Assert.NotNull(body!.ThumbnailUrl);
        Assert.Equal(ExpectedPresignedUrl(media.ThumbnailPath!), body.ThumbnailUrl);
    }

    [Fact]
    public async Task GetMedia_PdfType_ReturnsCorrectType()
    {
        var media = await SeedMediaAsync(MediaTypeEnum.Pdf);

        var response = await fixture.DoGet($"/api/media/{media.Id.Value}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetMediaResponse>();
        Assert.NotNull(body);
        Assert.Equal("Pdf", body!.Type);
    }

    [Fact]
    public async Task GetMedia_ImageType_ReturnsCorrectType()
    {
        var media = await SeedMediaAsync(MediaTypeEnum.Image);

        var response = await fixture.DoGet($"/api/media/{media.Id.Value}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetMediaResponse>();
        Assert.NotNull(body);
        Assert.Equal("Image", body!.Type);
    }

    [Fact]
    public async Task GetLessonMedia_WithNoMedia_ReturnsEmptyList()
    {
        var response = await fixture.DoGet($"/api/media/lesson/{BogusFactory.NewGuid()}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<GetMediaResponse>>();
        Assert.NotNull(body);
        Assert.Empty(body);
    }

    [Fact]
    public async Task GetLessonMedia_ReturnsPresignedFileUrl()
    {
        var media = await SeedMediaAsync();

        var response = await fixture.DoGet($"/api/media/lesson/{media.LessonId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<GetMediaResponse>>();
        Assert.NotNull(body);
        var item = Assert.Single(body);
        Assert.Equal(ExpectedPresignedUrl(media.FilePath), item.FileUrl);
    }

    [Fact]
    public async Task GetLessonMedia_WithThumbnail_ReturnsThumbnailUrl()
    {
        var media = await SeedMediaWithThumbnailAsync();

        var response = await fixture.DoGet($"/api/media/lesson/{media.LessonId}");

        var body = await response.Content.ReadFromJsonAsync<List<GetMediaResponse>>();
        var item = Assert.Single(body!);
        Assert.Equal(ExpectedPresignedUrl(media.ThumbnailPath!), item.ThumbnailUrl);
    }

    [Fact]
    public async Task GetLessonMedia_WithMultipleMedia_ReturnsAllWithUrls()
    {
        var lessonId = BogusFactory.NewGuid();
        var media1 = MediaEntity.Create(lessonId, MediaTypeEnum.Video, $"media/test/{BogusFactory.UniqueId()}.mp4");
        var media2 = MediaEntity.Create(lessonId, MediaTypeEnum.Pdf, $"media/test/{BogusFactory.UniqueId()}.pdf");
        fixture.DbContext.Media.AddRange(media1, media2);
        await fixture.DbContext.SaveChangesAsync();

        var response = await fixture.DoGet($"/api/media/lesson/{lessonId}");

        var body = await response.Content.ReadFromJsonAsync<List<GetMediaResponse>>();
        Assert.NotNull(body);
        Assert.Equal(2, body.Count);
        Assert.All(body, item => Assert.NotNull(item.FileUrl));
    }

    [Fact]
    public async Task UpdateStatus_ToProcessing_Returns200WithUpdatedStatus()
    {
        var media = await SeedMediaAsync();
        var token = NewUserToken();

        var response = await fixture.DoPatch(
            $"/api/media/{media.Id.Value}/status",
            new UpdateMediaStatusRequest("Processing"),
            token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetMediaResponse>();
        Assert.NotNull(body);
        Assert.Equal("Processing", body!.Status);
    }

    [Fact]
    public async Task UpdateStatus_ToReady_Returns200WithReadyStatus()
    {
        var media = await SeedMediaAsync();
        var token = NewUserToken();

        var response = await fixture.DoPatch(
            $"/api/media/{media.Id.Value}/status",
            new UpdateMediaStatusRequest("Ready"),
            token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetMediaResponse>();
        Assert.NotNull(body);
        Assert.Equal("Ready", body!.Status);
    }

    [Fact]
    public async Task UpdateStatus_ReturnsPresignedFileUrl()
    {
        var media = await SeedMediaAsync();
        var token = NewUserToken();

        var response = await fixture.DoPatch(
            $"/api/media/{media.Id.Value}/status",
            new UpdateMediaStatusRequest("Ready"),
            token);

        var body = await response.Content.ReadFromJsonAsync<GetMediaResponse>();
        Assert.NotNull(body!.FileUrl);
        Assert.Equal(ExpectedPresignedUrl(media.FilePath), body.FileUrl);
    }

    [Fact]
    public async Task UpdateStatus_WithInvalidStatus_Returns400()
    {
        var media = await SeedMediaAsync();
        var token = NewUserToken();

        var response = await fixture.DoPatch(
            $"/api/media/{media.Id.Value}/status",
            new UpdateMediaStatusRequest(BogusFactory.Faker.Lorem.Word()),
            token);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Upload_VideoFile_Returns201WithMediaData()
    {
        var lessonId = BogusFactory.NewGuid();
        var token = NewUserToken();

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes("fake video content"));
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp4");
        content.Add(fileContent, "file", "test-video.mp4");

        var client = fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsync(
            $"/api/media/upload?lessonId={lessonId}&type=Video", content);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetMediaResponse>();
        Assert.NotNull(body);
        Assert.Equal(lessonId, body!.LessonId);
        Assert.Equal("Video", body.Type);
        Assert.Equal("Uploaded", body.Status);
    }

    [Fact]
    public async Task Upload_ReturnsPresignedFileUrl()
    {
        var lessonId = BogusFactory.NewGuid();
        var token = NewUserToken();

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes("fake video content"));
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp4");
        content.Add(fileContent, "file", "test-video.mp4");

        var client = fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsync(
            $"/api/media/upload?lessonId={lessonId}&type=Video", content);

        var body = await response.Content.ReadFromJsonAsync<GetMediaResponse>();
        Assert.NotNull(body!.FileUrl);
        Assert.Contains("test-minio", body.FileUrl);
        Assert.Contains("wisdom-media", body.FileUrl);
        Assert.Contains("test-video.mp4", body.FileUrl);
    }
}
