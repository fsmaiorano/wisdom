using Microsoft.EntityFrameworkCore;
using Wisdom.BuildingBlocks;
using Wisdom.User.Domain.Entities;
using Wisdom.User.Domain.Repositories;
using Wisdom.User.Infrastructure.Data;
namespace Wisdom.User.Infrastructure.Repositories;

public class UserRepository(UserDbContext dbContext) : IUserRepository
{
    public async Task<Result<UserEntity?>> GetByKeycloakIdAsync(string keycloakId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.KeycloakId == keycloakId, cancellationToken);
            return Result<UserEntity?>.Success(user);
        }
        catch (Exception ex)
        {
            return Result<UserEntity?>.Failure(ex.Message);
        }
    }

    public async Task<Result<UserEntity?>> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
            return Result<UserEntity?>.Success(user);
        }
        catch (Exception ex)
        {
            return Result<UserEntity?>.Failure(ex.Message);
        }
    }

    public async Task<Result<bool>> ExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var exists = await dbContext.Users.AnyAsync(u => u.Email == email, cancellationToken);
            return Result<bool>.Success(exists);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }

    public async Task<Result<UserEntity>> CreateAsync(UserEntity user, CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.Users.AddAsync(user, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result<UserEntity>.Success(user);
        }
        catch (Exception ex)
        {
            return Result<UserEntity>.Failure(ex.Message);
        }
    }
}
