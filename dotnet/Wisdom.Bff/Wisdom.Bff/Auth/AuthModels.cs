using System.Text.Json.Serialization;

namespace Wisdom.Bff.Auth;

public record LoginRequest(string Email, string Password);

public record UserDto(string Id, string Email, string Name, string Role);

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    UserDto User);

internal record KeycloakTokenResponse(
    [property: JsonPropertyName("access_token")]  string AccessToken,
    [property: JsonPropertyName("refresh_token")] string? RefreshToken,
    [property: JsonPropertyName("expires_in")]    int ExpiresIn,
    [property: JsonPropertyName("token_type")]    string TokenType);
