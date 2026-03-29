namespace Wisdom.Simulator.Models;

public record RegisterUserRequest(string Email, string Name, string Password, int Role = 2);
public record RegisterUserResponse(Guid UserId, string KeycloakId, string Email, string Name, int Role);

public record LoginUserRequest(string Email, string Password);
public record LoginUserResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    int RefreshExpiresIn,
    string TokenType);

public record MeResponse(Guid UserId, string KeycloakId, string Email);
