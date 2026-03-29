using Wisdom.BuildingBlocks;
using Wisdom.User.Domain.Entities;
namespace Wisdom.User.Domain.Repositories;

public interface IUserRepository
{
    Task<Result<UserEntity?>> GetByKeycloakIdAsync(string keycloakId, CancellationToken cancellationToken = default);
    Task<Result<UserEntity?>> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Result<bool>> ExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<Result<UserEntity>> CreateAsync(UserEntity user, CancellationToken cancellationToken = default);
}