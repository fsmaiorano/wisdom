using Wisdom.BuildingBlocks;
using Wisdom.Order.Domain.Entities;
using Wisdom.Order.Domain.Repositories;
using Wisdom.Order.Domain.ValueObjects;
namespace Wisdom.Order.UnitTests.Mocks;

public class FakePurchaseRepository : IPurchaseRepository
{
    private readonly List<PurchaseEntity> _store = [];

    public bool CreateShouldFail { get; set; }
    public bool ExistsShouldFail { get; set; }
    public bool ExistsResult { get; set; }

    public int GetByIdCallCount { get; private set; }
    public int GetByUserIdCallCount { get; private set; }
    public int ExistsCallCount { get; private set; }
    public int CreateCallCount { get; private set; }

    public PurchaseEntity? LastCreatedPurchase { get; private set; }

    public IReadOnlyList<PurchaseEntity> All => _store.AsReadOnly();

    public FakePurchaseRepository Seed(params PurchaseEntity[] purchases)
    {
        _store.AddRange(purchases);
        return this;
    }

    public Task<Result<PurchaseEntity?>> GetByIdAsync(PurchaseId id, CancellationToken ct = default)
    {
        GetByIdCallCount++;
        var purchase = _store.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(Result<PurchaseEntity?>.Success(purchase));
    }

    public Task<Result<IReadOnlyList<PurchaseEntity>>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        GetByUserIdCallCount++;
        IReadOnlyList<PurchaseEntity> result = _store.Where(p => p.UserId == userId).ToList();
        return Task.FromResult(Result<IReadOnlyList<PurchaseEntity>>.Success(result));
    }

    public Task<Result<bool>> ExistsAsync(Guid userId, Guid courseId, CancellationToken ct = default)
    {
        ExistsCallCount++;

        if (ExistsShouldFail)
            return Task.FromResult(Result<bool>.Failure("Repository error: Exists failed."));

        var result = ExistsResult || _store.Any(p => p.UserId == userId && p.CourseId == courseId);
        return Task.FromResult(Result<bool>.Success(result));
    }

    public Task<Result<PurchaseEntity>> CreateAsync(PurchaseEntity purchase, CancellationToken ct = default)
    {
        CreateCallCount++;
        LastCreatedPurchase = purchase;

        if (CreateShouldFail)
            return Task.FromResult(Result<PurchaseEntity>.Failure("Repository error: Create failed."));

        _store.Add(purchase);
        return Task.FromResult(Result<PurchaseEntity>.Success(purchase));
    }
}
