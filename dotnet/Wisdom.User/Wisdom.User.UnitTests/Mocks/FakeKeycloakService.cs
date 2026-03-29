using Wisdom.BuildingBlocks;
using Wisdom.User.Application.Interfaces;
using Wisdom.User.Domain.Enums;
namespace Wisdom.User.UnitTests.Mocks;

public class FakeKeycloakService : IKeycloakService
{

    public Result<KeycloakRegisterResult> RegisterResult { get; set; } =
        Result<KeycloakRegisterResult>.Success(new KeycloakRegisterResult(Guid.NewGuid().ToString()));

    public Result<KeycloakTokenResult> LoginResult { get; set; } =
        Result<KeycloakTokenResult>.Success(new KeycloakTokenResult("access-token", "refresh-token", 300, 1800, "Bearer"));

    public Result<KeycloakTokenResult> RefreshResult { get; set; } =
        Result<KeycloakTokenResult>.Success(new KeycloakTokenResult("new-access-token", "new-refresh-token", 300, 1800, "Bearer"));

    public Result<bool> LogoutResult { get; set; } = Result<bool>.Success(true);
    public Result<bool> DeleteResult { get; set; } = Result<bool>.Success(true);

    public int RegisterCallCount { get; private set; }
    public int LoginCallCount    { get; private set; }
    public int RefreshCallCount  { get; private set; }
    public int LogoutCallCount   { get; private set; }
    public int DeleteCallCount   { get; private set; }

    public string?            LastRegisterEmail    { get; private set; }
    public string?            LastRegisterName     { get; private set; }
    public string?            LastRegisterPassword { get; private set; }
    public UserRoleEnum?      LastRegisterRole     { get; private set; }
    public string?            LastLoginEmail       { get; private set; }
    public string?            LastLoginPassword    { get; private set; }
    public string?            LastRefreshToken     { get; private set; }
    public string?            LastLogoutToken      { get; private set; }
    public string?            LastDeleteKeycloakId { get; private set; }
    public CancellationToken  LastCancellationToken { get; private set; }

    public Task<Result<KeycloakRegisterResult>> RegisterAsync(
        string email, string name, string password, UserRoleEnum role,
        CancellationToken cancellationToken = default)
    {
        RegisterCallCount++;
        LastRegisterEmail    = email;
        LastRegisterName     = name;
        LastRegisterPassword = password;
        LastRegisterRole     = role;
        LastCancellationToken = cancellationToken;
        return Task.FromResult(RegisterResult);
    }

    public Task<Result<KeycloakTokenResult>> LoginAsync(
        string email, string password,
        CancellationToken cancellationToken = default)
    {
        LoginCallCount++;
        LastLoginEmail    = email;
        LastLoginPassword = password;
        LastCancellationToken = cancellationToken;
        return Task.FromResult(LoginResult);
    }

    public Task<Result<KeycloakTokenResult>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        RefreshCallCount++;
        LastRefreshToken = refreshToken;
        LastCancellationToken = cancellationToken;
        return Task.FromResult(RefreshResult);
    }

    public Task<Result<bool>> LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        LogoutCallCount++;
        LastLogoutToken = refreshToken;
        LastCancellationToken = cancellationToken;
        return Task.FromResult(LogoutResult);
    }

    public Task<Result<bool>> DeleteUserAsync(
        string keycloakId,
        CancellationToken cancellationToken = default)
    {
        DeleteCallCount++;
        LastDeleteKeycloakId = keycloakId;
        LastCancellationToken = cancellationToken;
        return Task.FromResult(DeleteResult);
    }

    public Task<Result<bool>> SetUserAttributeAsync(
        string keycloakId, string key, string value,
        CancellationToken cancellationToken = default)
        => Task.FromResult(Result<bool>.Success(true));

    public Task<Result<bool>> EnsureUserIdClaimMapperAsync(
        CancellationToken cancellationToken = default)
        => Task.FromResult(Result<bool>.Success(true));
}
