using Wisdom.Course.Application.Dtos;
using Wisdom.Course.Application.UseCases;
using Wisdom.Course.UnitTests.Mocks;
using Wisdom.Tests.Common.Assertions;
namespace Wisdom.Course.UnitTests.Application.UseCases;

public class CreateCourseUseCaseTests
{
    private readonly FakeCourseRepository _repo = new();
    private readonly Guid _teacherId = Guid.NewGuid();

    private CreateCourseUseCase CreateUseCase() => new(_repo);

    private static CreateCourseRequest ValidRequest() =>
        new("Introduction to .NET", "A great course", 99.99m);

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_CreatesCourse()
    {
        var useCase = CreateUseCase();

        await useCase.ExecuteAsync(_teacherId, ValidRequest());

        Assert.Equal(1, _repo.CreateCallCount);
        Assert.NotNull(_repo.LastCreatedCourse);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_ReturnsSuccessWithMappedResponse()
    {
        var useCase = CreateUseCase();
        var request = ValidRequest();

        var result = await useCase.ExecuteAsync(_teacherId, request);

        var data = result.ShouldSucceed();
        Assert.NotEqual(Guid.Empty, data.CourseId);
        Assert.Equal(_teacherId, data.TeacherId);
        Assert.Equal(request.Title, data.Title);
        Assert.Equal(request.Description, data.Description);
        Assert.Equal(request.Price, data.Price);
        Assert.False(data.IsPublished);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRepositoryFails_ReturnsFailure()
    {
        _repo.CreateShouldFail = true;
        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(_teacherId, ValidRequest());

        result.ShouldFail("Repository error");
    }
}
