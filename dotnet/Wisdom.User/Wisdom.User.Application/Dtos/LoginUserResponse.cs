namespace Wisdom.User.Application.Dtos;
public record LoginUserResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    int RefreshExpiresIn,
    string TokenType);
