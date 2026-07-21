namespace Wisdom.Bff.Clients;

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
