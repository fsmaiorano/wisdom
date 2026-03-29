using Wisdom.BuildingBlocks;
using Wisdom.User.Application.Interfaces;
using Wisdom.User.Domain.Enums;
namespace Wisdom.User.IntegrationTests.Mocks;

public class TestKeycloakService : IKeycloakService
{
    public bool RegisterShouldFail { get; set; }
    public bool LoginShouldFail    { get; set; }
    public bool RefreshShouldFail  { get; set; }
    public bool LogoutShouldFail   { get; set; }
    public bool DeleteShouldFail   { get; set; }

    public string RegisteredKeycloakId { get; set; } = Guid.NewGuid().ToString();
    public string FakeAccessToken      { get; set; } = "fake-access-token";
    public string FakeRefreshToken     { get; set; } = "fake-refresh-token";

    public Task<Result<KeycloakRegisterResult>> RegisterAsync(
        string email, string name, string password, UserRoleEnum role,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(RegisterShouldFail
            ? Result<KeycloakRegisterResult>.Failure("Keycloak registration failed.")
            : Result<KeycloakRegisterResult>.Success(new KeycloakRegisterResult(RegisteredKeycloakId)));

    public Task<Result<KeycloakTokenResult>> LoginAsync(
        string email, string password,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(LoginShouldFail
            ? Result<KeycloakTokenResult>.Failure("Keycloak login failed.")
            : Result<KeycloakTokenResult>.Success(BuildToken()));

    public Task<Result<KeycloakTokenResult>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(RefreshShouldFail
            ? Result<KeycloakTokenResult>.Failure("Keycloak refresh failed.")
            : Result<KeycloakTokenResult>.Success(BuildToken()));

    public Task<Result<bool>> LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(LogoutShouldFail
            ? Result<bool>.Failure("Keycloak logout failed.")
            : Result<bool>.Success(true));

    public Task<Result<bool>> DeleteUserAsync(
        string keycloakId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(DeleteShouldFail
            ? Result<bool>.Failure("Keycloak delete failed.")
            : Result<bool>.Success(true));

    public Task<Result<bool>> SetUserAttributeAsync(
        string keycloakId, string key, string value,
        CancellationToken cancellationToken = default)
        => Task.FromResult(Result<bool>.Success(true));

    public Task<Result<bool>> EnsureUserIdClaimMapperAsync(
        CancellationToken cancellationToken = default)
        => Task.FromResult(Result<bool>.Success(true));

    private KeycloakTokenResult BuildToken() =>
        new(FakeAccessToken, FakeRefreshToken, 300, 1800, "Bearer");
}
