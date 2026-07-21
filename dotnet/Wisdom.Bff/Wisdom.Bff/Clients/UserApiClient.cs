namespace Wisdom.Bff.Clients;

public interface IUserApiClient
{
    Task<BackendRegisteredUser> RegisterAsync(RegisterUserRequest request, CancellationToken ct);
    Task<BackendAuthTokens> LoginAsync(LoginUserRequest request, CancellationToken ct);
    Task<BackendAuthTokens> RefreshAsync(RefreshTokenRequest request, CancellationToken ct);
    Task LogoutAsync(LogoutUserRequest request, CancellationToken ct);
    Task<BackendMe> GetMeAsync(CancellationToken ct);
}

// Wire-compatible with Wisdom.User.Domain.Enums.UserRoleEnum (Admin=0, Teacher=1, Customer=2).
// Not referenced directly — Bff doesn't take a project reference on Wisdom.User.
public enum BackendUserRole
{
    Admin = 0,
    Teacher = 1,
    Customer = 2,
}

public record RegisterUserRequest(string Email, string Name, string Password, BackendUserRole Role = BackendUserRole.Customer);
public record BackendRegisteredUser(Guid UserId, string KeycloakId, string Email, string Name, BackendUserRole Role);

public record LoginUserRequest(string Email, string Password);
public record RefreshTokenRequest(string RefreshToken);
public record LogoutUserRequest(string RefreshToken);
public record BackendAuthTokens(
    string AccessToken, string RefreshToken, int ExpiresIn, int RefreshExpiresIn, string TokenType);

public record BackendMe(Guid? UserId, string? KeycloakId, string Email);

public sealed class UserApiClient(HttpClient httpClient) : DownstreamClientBase(httpClient), IUserApiClient
{
    public Task<BackendRegisteredUser> RegisterAsync(RegisterUserRequest request, CancellationToken ct)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/auth/register") { Content = JsonContent(request) };
        return SendAsync<BackendRegisteredUser>(req, ct)!;
    }

    public Task<BackendAuthTokens> LoginAsync(LoginUserRequest request, CancellationToken ct)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login") { Content = JsonContent(request) };
        return SendAsync<BackendAuthTokens>(req, ct)!;
    }

    public Task<BackendAuthTokens> RefreshAsync(RefreshTokenRequest request, CancellationToken ct)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh") { Content = JsonContent(request) };
        return SendAsync<BackendAuthTokens>(req, ct)!;
    }

    public Task LogoutAsync(LogoutUserRequest request, CancellationToken ct)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout") { Content = JsonContent(request) };
        return SendAsync(req, ct);
    }

    public Task<BackendMe> GetMeAsync(CancellationToken ct)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, "/api/users/me");
        return SendAsync<BackendMe>(req, ct)!;
    }

    private static HttpContent JsonContent<T>(T value) => System.Net.Http.Json.JsonContent.Create(value, options: JsonOptions);
}
