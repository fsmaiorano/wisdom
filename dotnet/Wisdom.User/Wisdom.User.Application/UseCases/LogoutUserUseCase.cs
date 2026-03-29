using Wisdom.BuildingBlocks;
using Wisdom.BuildingBlocks.Loggers;
using Wisdom.User.Application.Dtos;
using Wisdom.User.Application.Interfaces;

namespace Wisdom.User.Application.UseCases;

public class LogoutUserUseCase(
    IKeycloakService keycloakService,
    IAppLogger<LogoutUserUseCase> logger)
{
    public async Task<Result<bool>> ExecuteAsync(LogoutUserRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(LogTypeEnum.Business, "Logout requested");

        var result = await keycloakService.LogoutAsync(request.RefreshToken, cancellationToken);

        if (!result.IsSuccess)
            logger.LogWarning(LogTypeEnum.Business, "Logout failed: {Error}", result.Error);
        else
            logger.LogInformation(LogTypeEnum.Business, "Logout successful");

        return result;
    }
}
