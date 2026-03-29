using Wisdom.BuildingBlocks;
using Wisdom.Tests.Common.Assertions;
using Wisdom.Tests.Common.Fakes;
using Wisdom.User.Application.Dtos;
using Wisdom.User.Application.Interfaces;
using Wisdom.User.Application.UseCases;
using Wisdom.User.UnitTests.Mocks;
namespace Wisdom.User.UnitTests.Application.UseCases;

public class LogoutUserUseCaseTests
{
    private readonly FakeKeycloakService _keycloak = new();
    private LogoutUserUseCase CreateUseCase() => new(_keycloak, new FakeAppLogger<LogoutUserUseCase>());

    [Fact]
    public async Task ExecuteAsync_WhenLogoutSucceeds_ReturnsSuccess()
    {
        _keycloak.LogoutResult = Result<bool>.Success(true);
        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(new LogoutUserRequest("valid-refresh-token"));

        var value = result.ShouldSucceed();
        Assert.True(value);
    }

    [Fact]
    public async Task ExecuteAsync_CallsKeycloakLogoutOnce()
    {
        var useCase = CreateUseCase();
        await useCase.ExecuteAsync(new LogoutUserRequest("some-token"));
        Assert.Equal(1, _keycloak.LogoutCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenLogoutFails_ReturnsFailure()
    {
        _keycloak.LogoutResult = Result<bool>.Failure("session not found");
        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(new LogoutUserRequest("expired-token"));

        result.ShouldFail("session not found");
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsRefreshTokenToKeycloak()
    {
        var useCase = CreateUseCase();
        await useCase.ExecuteAsync(new LogoutUserRequest("my-refresh-token"));

        Assert.Equal("my-refresh-token", _keycloak.LastLogoutToken);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var useCase   = CreateUseCase();

        await useCase.ExecuteAsync(new LogoutUserRequest("t"), cts.Token);

        Assert.Equal(cts.Token, _keycloak.LastCancellationToken);
    }
}
