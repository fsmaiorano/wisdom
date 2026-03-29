using Wisdom.BuildingBlocks;
using Wisdom.User.Domain.Enums;
namespace Wisdom.User.Application.Interfaces;

public record KeycloakRegisterResult(string KeycloakId);

public record KeycloakTokenResult(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    int RefreshExpiresIn,
    string TokenType);

public interface IKeycloakService
{
    Task<Result<KeycloakRegisterResult>> RegisterAsync(string email, string name, string password, UserRoleEnum role, CancellationToken cancellationToken = default);
    Task<Result<KeycloakTokenResult>> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<Result<KeycloakTokenResult>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<Result<bool>> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteUserAsync(string keycloakId, CancellationToken cancellationToken = default);

    /// <summary>Sets a single attribute on an existing Keycloak user.</summary>
    Task<Result<bool>> SetUserAttributeAsync(string keycloakId, string key, string value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates the "userId" → JWT claim protocol mapper on the client if it does not already exist.
    /// Idempotent — safe to call on every registration.
    /// </summary>
    Task<Result<bool>> EnsureUserIdClaimMapperAsync(CancellationToken cancellationToken = default);
}
