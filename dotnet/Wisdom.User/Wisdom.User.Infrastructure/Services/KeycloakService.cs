using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Wisdom.BuildingBlocks;
using Wisdom.BuildingBlocks.Http;
using Wisdom.User.Application.Interfaces;
using Wisdom.User.Domain.Enums;
using Wisdom.User.Infrastructure.Settings;
namespace Wisdom.User.Infrastructure.Services;

public class KeycloakService(HttpClient httpClient, KeycloakSettings settings)
    : BaseHttpService(httpClient), IKeycloakService
{
    public async Task<Result<KeycloakRegisterResult>> RegisterAsync(string email, string name, string password,
        UserRoleEnum role, CancellationToken cancellationToken = default)
    {
        try
        {
            var adminToken = await GetAdminTokenAsync();
            var parts = name.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var firstName = parts[0];
            var lastName = parts.Length > 1 ? parts[1] : parts[0];

            var payload = new
            {
                username = email,
                email,
                firstName,
                lastName,
                enabled = true,
                emailVerified = true,
                requiredActions = Array.Empty<string>(),
                credentials = new[] { new { type = "password", value = password, temporary = false } }
            };

            var response = await DoPost($"/admin/realms/{settings.Realm}/users", payload, adminToken);
            await EnsureKeycloakSuccessAsync(response);

            var location = response.Headers.Location?.ToString()
                ?? throw new InvalidOperationException("Keycloak did not return a user location header.");

            var keycloakUserId = location.Split('/').Last();

            await AssignRealmRoleAsync(keycloakUserId, role, adminToken);

            return Result<KeycloakRegisterResult>.Success(new KeycloakRegisterResult(keycloakUserId));
        }
        catch (Exception ex)
        {
            return Result<KeycloakRegisterResult>.Failure(ex.Message);
        }
    }

    public async Task<Result<KeycloakTokenResult>> LoginAsync(string email, string password,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var formData = new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = settings.ClientId,
                ["username"] = email,
                ["password"] = password,
                ["scope"] = "openid"
            };

            if (!string.IsNullOrWhiteSpace(settings.ClientSecret))
                formData["client_secret"] = settings.ClientSecret;

            var response = await DoPostFormUrlEncoded(
                $"/realms/{settings.Realm}/protocol/openid-connect/token", formData);

            await EnsureKeycloakSuccessAsync(response);
            return Result<KeycloakTokenResult>.Success(MapToTokenResult(await ReadTokenResponseAsync(response)));
        }
        catch (Exception ex)
        {
            return Result<KeycloakTokenResult>.Failure(ex.Message);
        }
    }

    public async Task<Result<KeycloakTokenResult>> RefreshTokenAsync(string refreshToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var formData = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["client_id"] = settings.ClientId,
                ["refresh_token"] = refreshToken
            };

            if (!string.IsNullOrWhiteSpace(settings.ClientSecret))
                formData["client_secret"] = settings.ClientSecret;

            var response = await DoPostFormUrlEncoded(
                $"/realms/{settings.Realm}/protocol/openid-connect/token", formData);

            await EnsureKeycloakSuccessAsync(response);
            return Result<KeycloakTokenResult>.Success(MapToTokenResult(await ReadTokenResponseAsync(response)));
        }
        catch (Exception ex)
        {
            return Result<KeycloakTokenResult>.Failure(ex.Message);
        }
    }

    public async Task<Result<bool>> LogoutAsync(string refreshToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var formData = new Dictionary<string, string>
            {
                ["client_id"] = settings.ClientId,
                ["refresh_token"] = refreshToken
            };

            if (!string.IsNullOrWhiteSpace(settings.ClientSecret))
                formData["client_secret"] = settings.ClientSecret;

            var response = await DoPostFormUrlEncoded(
                $"/realms/{settings.Realm}/protocol/openid-connect/logout", formData);

            await EnsureKeycloakSuccessAsync(response);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }

    public async Task<Result<bool>> DeleteUserAsync(string keycloakId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var adminToken = await GetAdminTokenAsync();
            var response = await DoDelete($"/admin/realms/{settings.Realm}/users/{keycloakId}", adminToken);
            await EnsureKeycloakSuccessAsync(response);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }

    public async Task<Result<bool>> SetUserAttributeAsync(string keycloakId, string key, string value,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var adminToken = await GetAdminTokenAsync();

            // GET the full user representation — a Keycloak PUT replaces the entire
            // object, so we must send back every field we want to keep.
            var getResp = await DoGet($"/admin/realms/{settings.Realm}/users/{keycloakId}", adminToken);
            await EnsureKeycloakSuccessAsync(getResp);
            var rawJson = await getResp.Content.ReadAsStringAsync(cancellationToken);

            // Deserialize into a mutable dictionary to preserve all existing fields
            var userDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(rawJson)
                           ?? throw new InvalidOperationException("Could not deserialize Keycloak user.");

            // Merge the new attribute into the existing attributes map
            var existingAttrs = new Dictionary<string, List<string>>();
            if (userDict.TryGetValue("attributes", out var attrsElement)
                && attrsElement.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in attrsElement.EnumerateObject())
                {
                    var vals = new List<string>();
                    foreach (var v in prop.Value.EnumerateArray())
                        vals.Add(v.GetString() ?? string.Empty);
                    existingAttrs[prop.Name] = vals;
                }
            }
            existingAttrs[key] = [value];

            // Rebuild the full payload using the original JSON as the base, then
            // override only the "attributes" property.
            var fullPayload = new Dictionary<string, object>();
            foreach (var kv in userDict)
                fullPayload[kv.Key] = kv.Value;

            fullPayload["attributes"] = existingAttrs;

            var putResp = await DoPut($"/admin/realms/{settings.Realm}/users/{keycloakId}", fullPayload, adminToken);
            await EnsureKeycloakSuccessAsync(putResp);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }

    public async Task<Result<bool>> EnsureUserIdClaimMapperAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var adminToken = await GetAdminTokenAsync();

            // Resolve the client's internal UUID from its clientId string
            var clientsResp = await DoGet(
                $"/admin/realms/{settings.Realm}/clients?clientId={Uri.EscapeDataString(settings.ClientId)}&max=1",
                adminToken);
            await EnsureKeycloakSuccessAsync(clientsResp);
            var clients = await clientsResp.Content.ReadFromJsonAsync<JsonElement[]>(cancellationToken: cancellationToken)
                          ?? [];

            if (clients.Length == 0)
                return Result<bool>.Failure($"Keycloak client '{settings.ClientId}' not found.");

            var clientUuid = clients[0].GetProperty("id").GetString()!;
            var mappersPath = $"/admin/realms/{settings.Realm}/clients/{clientUuid}/protocol-mappers/models";

            // Check if the mapper already exists
            var listResp = await DoGet(mappersPath, adminToken);
            await EnsureKeycloakSuccessAsync(listResp);
            var mappers = await listResp.Content.ReadFromJsonAsync<JsonElement[]>(cancellationToken: cancellationToken) ?? [];
            var exists = mappers.Any(m =>
                m.TryGetProperty("name", out var n) && n.GetString() == "userId");

            if (exists)
                return Result<bool>.Success(true);

            // Create the protocol mapper: user attribute "userId" → JWT claim "userId"
            var mapper = new
            {
                name = "userId",
                protocol = "openid-connect",
                protocolMapper = "oidc-usermodel-attribute-mapper",
                consentRequired = false,
                config = new Dictionary<string, string>
                {
                    ["user.attribute"]      = "userId",
                    ["claim.name"]          = "userId",
                    ["jsonType.label"]      = "String",
                    ["id.token.claim"]      = "true",
                    ["access.token.claim"]  = "true",
                    ["userinfo.token.claim"]= "true"
                }
            };

            var createResp = await DoPost(mappersPath, mapper, adminToken);
            await EnsureKeycloakSuccessAsync(createResp);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }

    private async Task AssignRealmRoleAsync(string keycloakUserId, UserRoleEnum role, string adminToken)
    {
        var roleName = role.ToString().ToLowerInvariant();

        var roleResponse = await DoGet($"/admin/realms/{settings.Realm}/roles/{roleName}", adminToken);
        await EnsureKeycloakSuccessAsync(roleResponse);

        var roleObj = await roleResponse.Content.ReadFromJsonAsync<JsonElement>();
        var roleId = roleObj.GetProperty("id").GetString()!;

        var assignPayload = new[] { new { id = roleId, name = roleName } };
        var assignResponse = await DoPost(
            $"/admin/realms/{settings.Realm}/users/{keycloakUserId}/role-mappings/realm",
            assignPayload, adminToken);
        await EnsureKeycloakSuccessAsync(assignResponse);
    }

    private async Task<string> GetAdminTokenAsync()
    {
        var response = await DoPostFormUrlEncoded(
            "/realms/master/protocol/openid-connect/token",
            new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = "admin-cli",
                ["username"] = settings.AdminUsername,
                ["password"] = settings.AdminPassword
            });

        await EnsureKeycloakSuccessAsync(response);
        return (await ReadTokenResponseAsync(response)).AccessToken;
    }

    private static async Task EnsureKeycloakSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync();

        try
        {
            var error = JsonSerializer.Deserialize<KeycloakErrorResponse>(body);
            var message = error?.ErrorDescription ?? error?.Error ?? body;
            throw new InvalidOperationException($"Keycloak error ({(int)response.StatusCode}): {message}");
        }
        catch (JsonException)
        {
            throw new InvalidOperationException($"Keycloak error ({(int)response.StatusCode}): {body}");
        }
    }

    private static async Task<KeycloakTokenResponse> ReadTokenResponseAsync(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>()
            ?? throw new InvalidOperationException("Failed to deserialize Keycloak token response.");

    private static KeycloakTokenResult MapToTokenResult(KeycloakTokenResponse r) =>
        new(r.AccessToken, r.RefreshToken, r.ExpiresIn, r.RefreshExpiresIn, r.TokenType);

    private sealed class KeycloakTokenResponse
    {
        [JsonPropertyName("access_token")] public string AccessToken { get; set; } = string.Empty;
        [JsonPropertyName("refresh_token")] public string RefreshToken { get; set; } = string.Empty;
        [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }
        [JsonPropertyName("refresh_expires_in")] public int RefreshExpiresIn { get; set; }
        [JsonPropertyName("token_type")] public string TokenType { get; set; } = string.Empty;
    }

    private sealed class KeycloakErrorResponse
    {
        [JsonPropertyName("error")] public string? Error { get; set; }
        [JsonPropertyName("error_description")] public string? ErrorDescription { get; set; }
    }
}
