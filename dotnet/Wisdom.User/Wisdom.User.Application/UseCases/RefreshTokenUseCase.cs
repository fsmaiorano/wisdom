using Wisdom.BuildingBlocks;
using Wisdom.BuildingBlocks.Loggers;
using Wisdom.User.Application.Dtos;
using Wisdom.User.Application.Interfaces;

namespace Wisdom.User.Application.UseCases;

public class RefreshTokenUseCase(
    IKeycloakService keycloakService,
    IAppLogger<RefreshTokenUseCase> logger)
{
    public async Task<Result<RefreshTokenResponse>> ExecuteAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogDebug(LogTypeEnum.Application, "Token refresh requested");

        var result = await keycloakService.RefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (!result.IsSuccess)
        {
            logger.LogWarning(LogTypeEnum.Application, "Token refresh failed: {Error}", result.Error);
            return Result<RefreshTokenResponse>.Failure(result.Error!);
        }

        var data = result.Data!;
        return Result<RefreshTokenResponse>.Success(
            new RefreshTokenResponse(data.AccessToken, data.RefreshToken, data.ExpiresIn, data.RefreshExpiresIn, data.TokenType));
    }
}
