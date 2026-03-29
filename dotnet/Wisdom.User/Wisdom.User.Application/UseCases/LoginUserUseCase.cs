using Wisdom.BuildingBlocks;
using Wisdom.BuildingBlocks.Loggers;
using Wisdom.User.Application.Dtos;
using Wisdom.User.Application.Interfaces;

namespace Wisdom.User.Application.UseCases;

public class LoginUserUseCase(
    IKeycloakService keycloakService,
    IAppLogger<LoginUserUseCase> logger)
{
    public async Task<Result<LoginUserResponse>> ExecuteAsync(LoginUserRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(LogTypeEnum.Business, "Login attempt for {Email}", request.Email);

        var result = await keycloakService.LoginAsync(request.Email, request.Password, cancellationToken);
        if (!result.IsSuccess)
        {
            logger.LogWarning(LogTypeEnum.Business, "Login failed for {Email}: {Error}", request.Email, result.Error);
            return Result<LoginUserResponse>.Failure(result.Error!);
        }

        logger.LogInformation(LogTypeEnum.Business, "Login successful for {Email}", request.Email);

        var data = result.Data!;
        return Result<LoginUserResponse>.Success(
            new LoginUserResponse(data.AccessToken, data.RefreshToken, data.ExpiresIn, data.RefreshExpiresIn, data.TokenType));
    }
}
