using Wisdom.Tests.Common.Assertions;
using Wisdom.Tests.Common.Builders;
using Wisdom.Order.Application.UseCases;
using Wisdom.Order.Domain.Entities;
using Wisdom.Order.UnitTests.Mocks;
namespace Wisdom.Order.UnitTests.Application.UseCases;

public class GetEnrollmentsUseCaseTests
{
    private readonly FakeEnrollmentRepository _enrollmentRepo = new();

    private GetEnrollmentsUseCase CreateUseCase() => new(_enrollmentRepo);

    [Fact]
    public async Task ExecuteAsync_ReturnsUserEnrollments()
    {
        var userId = BogusFactory.NewGuid();
        var courseId1 = BogusFactory.NewGuid();
        var courseId2 = BogusFactory.NewGuid();

        _enrollmentRepo.Seed(
            EnrollmentEntity.Create(userId, courseId1),
            EnrollmentEntity.Create(userId, courseId2),
            EnrollmentEntity.Create(BogusFactory.NewGuid(), BogusFactory.NewGuid()));

        var useCase = CreateUseCase();
        var result = await useCase.ExecuteAsync(userId);

        var data = result.ShouldSucceed();
        Assert.Equal(2, data.Count);
        Assert.All(data, r => Assert.Equal(userId, r.UserId));
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoEnrollments_ReturnsEmptyList()
    {
        var userId = BogusFactory.NewGuid();
        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(userId);

        var data = result.ShouldSucceed();
        Assert.Empty(data);
    }
}
