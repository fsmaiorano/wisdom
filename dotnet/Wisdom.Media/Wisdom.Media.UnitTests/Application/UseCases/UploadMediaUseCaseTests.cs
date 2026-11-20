using Wisdom.Media.Application.UseCases;
using Wisdom.Media.Domain.Enums;
using Wisdom.Media.UnitTests.Mocks;
using Wisdom.Tests.Common.Assertions;
using Wisdom.Tests.Common.Builders;
using Wisdom.Tests.Common.Fakes;

namespace Wisdom.Media.UnitTests.Application.UseCases;

public class UploadMediaUseCaseTests
{
    private readonly FakeMediaRepository _repo = new();
    private readonly FakeObjectStorageService _storage = new();
    private readonly FakeCorrelationIdService _correlationIdService = new();

    private UploadMediaUseCase CreateUseCase() => new(_repo, _storage, _correlationIdService);

    private static Stream CreateFakeStream(string content = "fake content")
        => new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_UploadsToStorage()
    {
        var lessonId = BogusFactory.NewGuid();
        var courseId = BogusFactory.NewGuid();
        var useCase = CreateUseCase();

        await using var stream = CreateFakeStream();
        await useCase.ExecuteAsync(lessonId, courseId, MediaTypeEnum.Video, stream, "video.mp4", "video/mp4");

        Assert.Equal(1, _storage.UploadCallCount);
        Assert.Equal("wisdom-media", _storage.LastBucketName);
        Assert.Contains("media-raw", _storage.LastObjectName);
        Assert.Contains(courseId.ToString(), _storage.LastObjectName);
        Assert.Contains("video.mp4", _storage.LastObjectName);
        Assert.Equal("video/mp4", _storage.LastContentType);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_CreatesMediaEntity()
    {
        var lessonId = BogusFactory.NewGuid();
        var courseId = BogusFactory.NewGuid();
        var useCase = CreateUseCase();

        await using var stream = CreateFakeStream();
        await useCase.ExecuteAsync(lessonId, courseId, MediaTypeEnum.Document, stream, "document.pdf", "application/pdf");

        Assert.Equal(1, _repo.CreateCallCount);
        Assert.NotNull(_repo.LastCreatedMedia);
        Assert.Equal(lessonId, _repo.LastCreatedMedia!.LessonId);
        Assert.Equal(MediaTypeEnum.Document, _repo.LastCreatedMedia.Type);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_ReturnsSuccessWithMappedResponse()
    {
        var lessonId = BogusFactory.NewGuid();
        var courseId = BogusFactory.NewGuid();
        var useCase = CreateUseCase();

        await using var stream = CreateFakeStream();
        var result = await useCase.ExecuteAsync(lessonId, courseId, MediaTypeEnum.Image, stream, "image.png", "image/png");

        var data = result.ShouldSucceed();
        Assert.NotEqual(Guid.Empty, data.MediaId);
        Assert.Equal(lessonId, data.LessonId);
        Assert.Equal("Image", data.Type);
        Assert.Equal("Uploaded", data.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStorageFails_ReturnsFailure()
    {
        _storage.UploadShouldFail = true;
        var useCase = CreateUseCase();

        await using var stream = CreateFakeStream();
        var result = await useCase.ExecuteAsync(BogusFactory.NewGuid(), BogusFactory.NewGuid(), MediaTypeEnum.Video,
            stream, "video.mp4", "video/mp4");

        result.ShouldFail("Storage error");
    }

    [Fact]
    public async Task ExecuteAsync_WhenStorageFails_DoesNotCallRepository()
    {
        _storage.UploadShouldFail = true;
        var useCase = CreateUseCase();

        await using var stream = CreateFakeStream();
        await useCase.ExecuteAsync(BogusFactory.NewGuid(), BogusFactory.NewGuid(), MediaTypeEnum.Video, stream,
            "video.mp4", "video/mp4");

        Assert.Equal(0, _repo.CreateCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRepositoryFails_ReturnsFailure()
    {
        _repo.CreateShouldFail = true;
        var useCase = CreateUseCase();

        await using var stream = CreateFakeStream();
        var result = await useCase.ExecuteAsync(BogusFactory.NewGuid(), BogusFactory.NewGuid(), MediaTypeEnum.Video,
            stream, "video.mp4", "video/mp4");

        result.ShouldFail("Repository error");
    }
}