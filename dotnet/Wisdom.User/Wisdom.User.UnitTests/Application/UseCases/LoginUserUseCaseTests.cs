using Wisdom.BuildingBlocks;
using Wisdom.Tests.Common.Assertions;
using Wisdom.Tests.Common.Fakes;
using Wisdom.User.Application.Dtos;
using Wisdom.User.Application.Interfaces;
using Wisdom.User.Application.UseCases;
using Wisdom.User.UnitTests.Mocks;
namespace Wisdom.User.UnitTests.Application.UseCases;

public class LoginUserUseCaseTests
{
    private readonly FakeKeycloakService _keycloak = new();
    private LoginUserUseCase CreateUseCase() => new(_keycloak, new FakeAppLogger<LoginUserUseCase>());

    [Fact]
    public async Task ExecuteAsync_WhenLoginSucceeds_ReturnsMappedTokenResponse()
    {
        _keycloak.LoginResult = Result<KeycloakTokenResult>.Success(
            new KeycloakTokenResult("access-tok", "refresh-tok", 300, 1800, "Bearer"));
        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(new LoginUserRequest("user@test.com", "pass"));

        var data = result.ShouldSucceed();
        Assert.Equal("access-tok",  data.AccessToken);
        Assert.Equal("refresh-tok", data.RefreshToken);
        Assert.Equal(300,           data.ExpiresIn);
        Assert.Equal(1800,          data.RefreshExpiresIn);
        Assert.Equal("Bearer",      data.TokenType);
    }

    [Fact]
    public async Task ExecuteAsync_CallsKeycloakLoginOnce()
    {
        var useCase = CreateUseCase();
        await useCase.ExecuteAsync(new LoginUserRequest("u@t.com", "p"));
        Assert.Equal(1, _keycloak.LoginCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenLoginFails_ReturnsFailure()
    {
        _keycloak.LoginResult = Result<KeycloakTokenResult>.Failure("invalid credentials");
        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(new LoginUserRequest("u@t.com", "wrong"));

        result.ShouldFail("invalid credentials");
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsEmailAndPasswordToKeycloak()
    {
        var useCase = CreateUseCase();
        await useCase.ExecuteAsync(new LoginUserRequest("fwd@test.com", "mypass"));

        Assert.Equal("fwd@test.com", _keycloak.LastLoginEmail);
        Assert.Equal("mypass",       _keycloak.LastLoginPassword);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var useCase   = CreateUseCase();

        await useCase.ExecuteAsync(new LoginUserRequest("u@t.com", "p"), cts.Token);

        Assert.Equal(cts.Token, _keycloak.LastCancellationToken);
    }
}
