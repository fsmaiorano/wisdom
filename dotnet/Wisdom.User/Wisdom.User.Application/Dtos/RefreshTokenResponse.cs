namespace Wisdom.User.Application.Dtos;
public record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    int RefreshExpiresIn,
    string TokenType);
