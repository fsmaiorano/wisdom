using Wisdom.BuildingBlocks;
using Wisdom.BuildingBlocks.Loggers;
using Wisdom.User.Domain.Entities;
using Wisdom.User.Domain.Repositories;

namespace Wisdom.User.Application.UseCases;

public class MeUseCase(
    IUserRepository userRepository,
    IAppLogger<MeUseCase> logger)
{
    public async Task<Result<UserEntity?>> ExecuteAsync(string email, CancellationToken cancellationToken = default)
    {
        logger.LogDebug(LogTypeEnum.Application, "Fetching profile for {Email}", email);
        return await userRepository.GetByEmailAsync(email, cancellationToken);
    }
}