using Wisdom.Tests.Common.Assertions;
using Wisdom.Tests.Common.Builders;
using Wisdom.Order.Application.Dtos;
using Wisdom.Order.Application.UseCases;
using Wisdom.Order.UnitTests.Mocks;
namespace Wisdom.Order.UnitTests.Application.UseCases;

public class PurchaseCourseUseCaseTests
{
    private readonly FakePurchaseRepository _purchaseRepo = new();
    private readonly FakeEnrollmentRepository _enrollmentRepo = new();

    private PurchaseCourseUseCase CreateUseCase() =>
        new(_purchaseRepo, _enrollmentRepo);

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_CreatesPurchaseAndEnrollment()
    {
        var userId = BogusFactory.NewGuid();
        var useCase = CreateUseCase();
        var request = new PurchaseCourseRequest(BogusFactory.NewGuid(), 49.99m);

        await useCase.ExecuteAsync(userId, request);

        Assert.Equal(1, _purchaseRepo.CreateCallCount);
        Assert.Equal(1, _enrollmentRepo.CreateCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_ReturnsSuccessResponse()
    {
        var userId = BogusFactory.NewGuid();
        var courseId = BogusFactory.NewGuid();
        var useCase = CreateUseCase();
        var request = new PurchaseCourseRequest(courseId, 99.99m);

        var result = await useCase.ExecuteAsync(userId, request);

        var data = result.ShouldSucceed();
        Assert.NotEqual(Guid.Empty, data.PurchaseId);
        Assert.Equal(userId, data.UserId);
        Assert.Equal(courseId, data.CourseId);
        Assert.Equal(99.99m, data.Price);
        Assert.NotNull(data.CreatedAt);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAlreadyPurchased_ReturnsFailure()
    {
        _purchaseRepo.ExistsResult = true;
        var useCase = CreateUseCase();
        var request = new PurchaseCourseRequest(BogusFactory.NewGuid(), 49.99m);

        var result = await useCase.ExecuteAsync(BogusFactory.NewGuid(), request);

        result.ShouldFail("Already purchased");
        Assert.Equal(0, _purchaseRepo.CreateCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPurchaseRepositoryFails_ReturnsFailure()
    {
        _purchaseRepo.CreateShouldFail = true;
        var useCase = CreateUseCase();
        var request = new PurchaseCourseRequest(BogusFactory.NewGuid(), 49.99m);

        var result = await useCase.ExecuteAsync(BogusFactory.NewGuid(), request);

        result.ShouldFail("Repository error");
        Assert.Equal(0, _enrollmentRepo.CreateCallCount);
    }
}
