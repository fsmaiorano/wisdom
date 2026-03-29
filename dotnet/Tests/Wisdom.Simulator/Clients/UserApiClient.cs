using Wisdom.Simulator.Models;

namespace Wisdom.Simulator.Clients;

public class UserApiClient(string baseUrl) : ApiClientBase(baseUrl)
{
    public Task<RegisterUserResponse?> RegisterAsync(RegisterUserRequest request, CancellationToken ct = default)
        => PostAsync<RegisterUserResponse>("/api/auth/register", request, ct);

    public Task<LoginUserResponse?> LoginAsync(LoginUserRequest request, CancellationToken ct = default)
        => PostAsync<LoginUserResponse>("/api/auth/login", request, ct);

    public Task<MeResponse?> MeAsync(CancellationToken ct = default)
        => GetAsync<MeResponse>("/api/users/me", ct);
}
