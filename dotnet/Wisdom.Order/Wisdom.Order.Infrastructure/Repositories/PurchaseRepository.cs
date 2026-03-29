using Microsoft.EntityFrameworkCore;
using Wisdom.BuildingBlocks;
using Wisdom.Order.Domain.Entities;
using Wisdom.Order.Domain.Repositories;
using Wisdom.Order.Domain.ValueObjects;
using Wisdom.Order.Infrastructure.Data;
namespace Wisdom.Order.Infrastructure.Repositories;

public class PurchaseRepository(OrderDbContext dbContext) : IPurchaseRepository
{
    public async Task<Result<PurchaseEntity?>> GetByIdAsync(PurchaseId id, CancellationToken ct = default)
    {
        try
        {
            var purchase = await dbContext.Purchases
                .FirstOrDefaultAsync(p => p.Id == id, ct);
            return Result<PurchaseEntity?>.Success(purchase);
        }
        catch (Exception ex)
        {
            return Result<PurchaseEntity?>.Failure(ex.Message);
        }
    }

    public async Task<Result<IReadOnlyList<PurchaseEntity>>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        try
        {
            var purchases = await dbContext.Purchases
                .Where(p => p.UserId == userId)
                .ToListAsync(ct);
            return Result<IReadOnlyList<PurchaseEntity>>.Success(purchases);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<PurchaseEntity>>.Failure(ex.Message);
        }
    }

    public async Task<Result<bool>> ExistsAsync(Guid userId, Guid courseId, CancellationToken ct = default)
    {
        try
        {
            var exists = await dbContext.Purchases
                .AnyAsync(p => p.UserId == userId && p.CourseId == courseId, ct);
            return Result<bool>.Success(exists);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }

    public async Task<Result<PurchaseEntity>> CreateAsync(PurchaseEntity purchase, CancellationToken ct = default)
    {
        try
        {
            await dbContext.Purchases.AddAsync(purchase, ct);
            await dbContext.SaveChangesAsync(ct);
            return Result<PurchaseEntity>.Success(purchase);
        }
        catch (Exception ex)
        {
            return Result<PurchaseEntity>.Failure(ex.Message);
        }
    }
}
