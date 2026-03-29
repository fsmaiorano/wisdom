using Wisdom.BuildingBlocks;
using Wisdom.Order.Domain.Entities;
using Wisdom.Order.Domain.ValueObjects;
namespace Wisdom.Order.Domain.Repositories;

public interface IPurchaseRepository
{
    Task<Result<PurchaseEntity?>> GetByIdAsync(PurchaseId id, CancellationToken ct = default);
    Task<Result<IReadOnlyList<PurchaseEntity>>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Result<bool>> ExistsAsync(Guid userId, Guid courseId, CancellationToken ct = default);
    Task<Result<PurchaseEntity>> CreateAsync(PurchaseEntity purchase, CancellationToken ct = default);
}
