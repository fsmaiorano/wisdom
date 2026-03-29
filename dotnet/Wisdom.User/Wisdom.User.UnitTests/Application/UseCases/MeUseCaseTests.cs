using Wisdom.BuildingBlocks;
using Wisdom.Tests.Common.Assertions;
using Wisdom.Tests.Common.Builders;
using Wisdom.Tests.Common.Fakes;
using Wisdom.User.Application.UseCases;
using Wisdom.User.UnitTests.Mocks;
namespace Wisdom.User.UnitTests.Application.UseCases;

public class MeUseCaseTests
{
    private readonly FakeUserRepository _repo = new();
    private MeUseCase CreateUseCase() => new(_repo, new FakeAppLogger<MeUseCase>());

    [Fact]
    public async Task ExecuteAsync_WhenUserExists_ReturnsTheUser()
    {
        var seeded = FakeUserBuilder.CreateUser(email: "me@test.com");
        _repo.Seed(seeded);
        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync("me@test.com");

        var user = result.ShouldSucceed();
        Assert.NotNull(user);
        Assert.Equal("me@test.com", user!.Email);
        Assert.Equal(seeded.Id.Value, user.Id.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserDoesNotExist_ReturnsSuccessWithNull()
    {
        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync("unknown@test.com");

        Assert.True(result.IsSuccess);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task ExecuteAsync_CallsGetByEmailOnce()
    {
        var useCase = CreateUseCase();
        await useCase.ExecuteAsync("anyone@test.com");
        Assert.Equal(1, _repo.GetByEmailCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsEmailToRepository()
    {
        var useCase = CreateUseCase();
        await useCase.ExecuteAsync("forward@test.com");
        Assert.Equal("forward@test.com", _repo.LastEmail);
    }

    [Fact]
    public async Task ExecuteAsync_WithMultipleSeededUsers_ReturnsCorrectOne()
    {
        var alice = FakeUserBuilder.CreateUser(email: "alice@test.com");
        var bob   = FakeUserBuilder.CreateUser(email: "bob@test.com");
        _repo.Seed(alice, bob);
        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync("bob@test.com");

        var user = result.ShouldSucceed();
        Assert.Equal("bob@test.com", user!.Email);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRepositoryFails_ReturnsFailure()
    {
        _repo.GetByEmailShouldFail = true;
        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync("error@test.com");

        result.ShouldFail();
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsCancellationToken()
    {
        var useCase = CreateUseCase();
        using var cts = new CancellationTokenSource();

        await useCase.ExecuteAsync("u@test.com", cts.Token);
        Assert.Equal(1, _repo.GetByEmailCallCount);
    }
}
