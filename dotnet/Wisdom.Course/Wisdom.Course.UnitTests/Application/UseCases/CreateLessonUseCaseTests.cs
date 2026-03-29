using Wisdom.Course.Application.Dtos;
using Wisdom.Course.Application.UseCases;
using Wisdom.Course.UnitTests.Mocks;
using Wisdom.Tests.Common.Assertions;
namespace Wisdom.Course.UnitTests.Application.UseCases;

public class CreateLessonUseCaseTests
{
    private readonly FakeLessonRepository _repo = new();
    private readonly Guid _courseId = Guid.NewGuid();

    private CreateLessonUseCase CreateUseCase() => new(_repo);

    private static CreateLessonRequest ValidRequest() =>
        new("Lesson 1", "First lesson content", 1, true);

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_CreatesLesson()
    {
        var useCase = CreateUseCase();

        await useCase.ExecuteAsync(_courseId, ValidRequest());

        Assert.Equal(1, _repo.CreateCallCount);
        Assert.NotNull(_repo.LastCreatedLesson);
        Assert.Equal(_courseId, _repo.LastCreatedLesson!.CourseId.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_ReturnsSuccessWithMappedResponse()
    {
        var useCase = CreateUseCase();
        var request = ValidRequest();

        var result = await useCase.ExecuteAsync(_courseId, request);

        var data = result.ShouldSucceed();
        Assert.NotEqual(Guid.Empty, data.LessonId);
        Assert.Equal(_courseId, data.CourseId);
        Assert.Equal(request.Title, data.Title);
        Assert.Equal(request.Description, data.Description);
        Assert.Equal(request.Position, data.Position);
        Assert.Equal(request.IsPreview, data.IsPreview);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRepositoryFails_ReturnsFailure()
    {
        _repo.CreateShouldFail = true;
        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(_courseId, ValidRequest());

        result.ShouldFail("Repository error");
    }
}
