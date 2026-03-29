using Wisdom.Tests.Common.Assertions;
using Wisdom.Tests.Common.Builders;
using Wisdom.Learning.Application.UseCases;
using Wisdom.Learning.Domain.Entities;
using Wisdom.Learning.UnitTests.Mocks;
namespace Wisdom.Learning.UnitTests.Application.UseCases;

public class GetProgressUseCaseTests
{
    private readonly FakeProgressRepository _repo = new();
    private readonly Guid _userId = BogusFactory.NewGuid();

    private GetProgressUseCase CreateUseCase() => new(_repo);

    [Fact]
    public async Task ExecuteAsync_WhenProgressExists_ReturnsProgress()
    {
        var lessonId = BogusFactory.NewGuid();
        var existing = ProgressEntity.Create(_userId, lessonId);
        existing.Update(90, true);
        _repo.Seed(existing);

        var useCase = CreateUseCase();
        var result = await useCase.ExecuteAsync(_userId, lessonId);

        var data = result.ShouldSucceed();
        Assert.Equal(lessonId, data.LessonId);
        Assert.Equal(90, data.WatchedSeconds);
        Assert.True(data.IsCompleted);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoProgress_ReturnsEmptyProgress()
    {
        var lessonId = BogusFactory.NewGuid();
        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(_userId, lessonId);

        var data = result.ShouldSucceed();
        Assert.Equal(lessonId, data.LessonId);
        Assert.Equal(0, data.WatchedSeconds);
        Assert.False(data.IsCompleted);
        Assert.Equal(Guid.Empty, data.ProgressId);
    }
}
