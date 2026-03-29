using Wisdom.Tests.Common.Assertions;
using Wisdom.Tests.Common.Builders;
using Wisdom.Learning.Application.Dtos;
using Wisdom.Learning.Application.UseCases;
using Wisdom.Learning.Domain.Entities;
using Wisdom.Learning.UnitTests.Mocks;
namespace Wisdom.Learning.UnitTests.Application.UseCases;

public class UpdateProgressUseCaseTests
{
    private readonly FakeProgressRepository _repo = new();
    private readonly Guid _userId = BogusFactory.NewGuid();

    private UpdateProgressUseCase CreateUseCase() => new(_repo);

    private static UpdateProgressRequest CreateRequest(Guid? lessonId = null, int watchedSeconds = 60, bool isCompleted = false)
        => new(lessonId ?? BogusFactory.NewGuid(), watchedSeconds, isCompleted);

    [Fact]
    public async Task ExecuteAsync_WhenNoExistingProgress_CreatesNewProgress()
    {
        var request = CreateRequest();
        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(_userId, request);

        result.ShouldSucceed();
        Assert.Equal(1, _repo.CreateCallCount);
        Assert.Equal(0, _repo.UpdateCallCount);
        Assert.NotNull(_repo.LastCreatedProgress);
    }

    [Fact]
    public async Task ExecuteAsync_WhenExistingProgress_UpdatesProgress()
    {
        var lessonId = BogusFactory.NewGuid();
        var existing = ProgressEntity.Create(_userId, lessonId);
        _repo.Seed(existing);

        var request = CreateRequest(lessonId, watchedSeconds: 120);
        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(_userId, request);

        result.ShouldSucceed();
        Assert.Equal(0, _repo.CreateCallCount);
        Assert.Equal(1, _repo.UpdateCallCount);
        Assert.Equal(120, _repo.LastUpdatedProgress!.WatchedSeconds);
    }

    [Fact]
    public async Task ExecuteAsync_WithCompletedLesson_SetsIsCompletedTrue()
    {
        var request = CreateRequest(isCompleted: true);
        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(_userId, request);

        var data = result.ShouldSucceed();
        Assert.True(data.IsCompleted);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRepositoryCreateFails_ReturnsFailure()
    {
        _repo.CreateShouldFail = true;
        var useCase = CreateUseCase();
        var request = CreateRequest();

        var result = await useCase.ExecuteAsync(_userId, request);

        result.ShouldFail("Repository error");
    }
}
