using Wisdom.Media.Application.Dtos;
using Wisdom.Media.Application.UseCases;
using Wisdom.Media.Domain.Entities;
using Wisdom.Media.Domain.Enums;
using Wisdom.Media.UnitTests.Mocks;
using Wisdom.Tests.Common.Assertions;
using Wisdom.Tests.Common.Builders;
using Wisdom.Tests.Common.Fakes;
namespace Wisdom.Media.UnitTests.Application.UseCases;

public class UpdateMediaStatusUseCaseTests
{
    private readonly FakeMediaRepository _repo = new();
    private readonly FakeObjectStorageService _storage = new();

    private UpdateMediaStatusUseCase CreateUseCase() => new(_repo, _storage);

    private MediaEntity CreateAndSeedMedia(Guid? lessonId = null)
    {
        var media = MediaEntity.Create(FakeCorrelationIdService.DefaultCorrelationId, lessonId ?? BogusFactory.NewGuid(), MediaTypeEnum.Video, "media/path/video.mp4");
        _repo.Seed(media);
        return media;
    }

    [Fact]
    public async Task ExecuteAsync_WithValidStatus_UpdatesMedia()
    {
        var media = CreateAndSeedMedia();
        var request = new UpdateMediaStatusRequest("Processing");
        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(media.Id.Value, request);

        var data = result.ShouldSucceed();
        Assert.Equal("Processing", data.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidStatus_CallsRepositoryUpdate()
    {
        var media = CreateAndSeedMedia();
        var useCase = CreateUseCase();

        await useCase.ExecuteAsync(media.Id.Value, new UpdateMediaStatusRequest("Ready"));

        Assert.Equal(1, _repo.UpdateCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidStatus_ReturnsFailure()
    {
        var media = CreateAndSeedMedia();
        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(media.Id.Value, new UpdateMediaStatusRequest("InvalidStatus"));

        result.ShouldFail("Invalid status value");
    }

    [Fact]
    public async Task ExecuteAsync_WhenMediaNotFound_ReturnsFailure()
    {
        var useCase = CreateUseCase();
        var nonExistentId = BogusFactory.NewGuid();

        var result = await useCase.ExecuteAsync(nonExistentId, new UpdateMediaStatusRequest("Processing"));

        result.ShouldFail("was not found");
    }

    [Fact]
    public async Task ExecuteAsync_WithStatusCaseInsensitive_Succeeds()
    {
        var media = CreateAndSeedMedia();
        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(media.Id.Value, new UpdateMediaStatusRequest("failed"));

        var data = result.ShouldSucceed();
        Assert.Equal("Failed", data.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRepositoryGetFails_ReturnsFailure()
    {
        _repo.GetByIdShouldFail = true;
        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(BogusFactory.NewGuid(), new UpdateMediaStatusRequest("Processing"));

        result.ShouldFail("Repository error");
    }
}
