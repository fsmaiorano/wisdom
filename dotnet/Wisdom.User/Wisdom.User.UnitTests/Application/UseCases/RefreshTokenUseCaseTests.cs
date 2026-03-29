using Wisdom.BuildingBlocks;
using Wisdom.Tests.Common.Assertions;
using Wisdom.Tests.Common.Fakes;
using Wisdom.User.Application.Dtos;
using Wisdom.User.Application.Interfaces;
using Wisdom.User.Application.UseCases;
using Wisdom.User.UnitTests.Mocks;
namespace Wisdom.User.UnitTests.Application.UseCases;

public class RefreshTokenUseCaseTests
{
    private readonly FakeKeycloakService _keycloak = new();
    private RefreshTokenUseCase CreateUseCase() => new(_keycloak, new FakeAppLogger<RefreshTokenUseCase>());

    [Fact]
    public async Task ExecuteAsync_WhenRefreshSucceeds_ReturnsMappedTokenResponse()
    {
        _keycloak.RefreshResult = Result<KeycloakTokenResult>.Success(
            new KeycloakTokenResult("new-access", "new-refresh", 600, 3600, "Bearer"));
        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(new RefreshTokenRequest("old-refresh-token"));

        var data = result.ShouldSucceed();
        Assert.Equal("new-access",  data.AccessToken);
        Assert.Equal("new-refresh", data.RefreshToken);
        Assert.Equal(600,           data.ExpiresIn);
        Assert.Equal(3600,          data.RefreshExpiresIn);
        Assert.Equal("Bearer",      data.TokenType);
    }

    [Fact]
    public async Task ExecuteAsync_CallsKeycloakRefreshOnce()
    {
        var useCase = CreateUseCase();
        await useCase.ExecuteAsync(new RefreshTokenRequest("tok"));
        Assert.Equal(1, _keycloak.RefreshCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenKeycloakFails_ReturnsFailure()
    {
        _keycloak.RefreshResult = Result<KeycloakTokenResult>.Failure("token expired");
        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(new RefreshTokenRequest("expired-token"));

        result.ShouldFail("token expired");
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsRefreshTokenToKeycloak()
    {
        var useCase = CreateUseCase();
        await useCase.ExecuteAsync(new RefreshTokenRequest("my-refresh-token"));

        Assert.Equal("my-refresh-token", _keycloak.LastRefreshToken);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var useCase   = CreateUseCase();

        await useCase.ExecuteAsync(new RefreshTokenRequest("t"), cts.Token);

        Assert.Equal(cts.Token, _keycloak.LastCancellationToken);
    }
}
