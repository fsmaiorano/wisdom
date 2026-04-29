using System.Text;
using System.Text.Json;

namespace Wisdom.Bff.Auth;

public sealed class AuthHandler(IHttpClientFactory httpFactory, IConfiguration config)
{
    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var keycloakBase = config["Keycloak:BaseUrl"]!;
        var realm        = config["Keycloak:Realm"]!;
        var clientId     = config["Keycloak:ClientId"] ?? "wisdom-app";

        var tokenUrl = $"{keycloakBase}/realms/{realm}/protocol/openid-connect/token";

        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"]  = clientId,
            ["username"]   = request.Email,
            ["password"]   = request.Password,
            ["scope"]      = "openid profile email",
        };

        var client   = httpFactory.CreateClient("keycloak");
        var response = await client.PostAsync(tokenUrl, new FormUrlEncodedContent(form), ct);

        if (!response.IsSuccessStatusCode)
            return null;

        var json      = await response.Content.ReadAsStringAsync(ct);
        var tokenData = JsonSerializer.Deserialize<KeycloakTokenResponse>(json);
        if (tokenData is null) return null;

        var payload = DecodeJwtPayload(tokenData.AccessToken);

        var userId = payload.TryGetProperty("userId", out var uidProp) ? uidProp.GetString()
                   : payload.TryGetProperty("sub",    out var subProp) ? subProp.GetString()
                   : null;

        var email = payload.TryGetProperty("email", out var emailProp)
            ? emailProp.GetString() ?? request.Email
            : request.Email;

        var name = payload.TryGetProperty("name", out var nameProp) ? nameProp.GetString()
                 : payload.TryGetProperty("preferred_username", out var unProp) ? unProp.GetString()
                 : email;

        var role = ExtractRole(payload);

        var user = new UserDto(userId ?? string.Empty, email, name ?? email, role);
        return new LoginResponse(tokenData.AccessToken, tokenData.RefreshToken ?? string.Empty, tokenData.ExpiresIn, user);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static JsonElement DecodeJwtPayload(string token)
    {
        var parts   = token.Split('.');
        var payload = parts[1];

        // Re-pad Base64Url to standard Base64
        var padded = (payload.Length % 4) switch
        {
            2 => payload + "==",
            3 => payload + "=",
            _ => payload,
        };

        var bytes = Convert.FromBase64String(padded.Replace('-', '+').Replace('_', '/'));
        return JsonSerializer.Deserialize<JsonElement>(Encoding.UTF8.GetString(bytes));
    }

    private static string ExtractRole(JsonElement payload)
    {
        try
        {
            if (payload.TryGetProperty("realm_access", out var realmAccess) &&
                realmAccess.TryGetProperty("roles", out var rolesEl))
            {
                foreach (var role in rolesEl.EnumerateArray())
                {
                    var r = role.GetString();
                    if (r is "teacher")  return "teacher";
                    if (r is "customer") return "customer";
                }
            }
        }
        catch { /* ignore malformed claim */ }

        return "customer";
    }
}
