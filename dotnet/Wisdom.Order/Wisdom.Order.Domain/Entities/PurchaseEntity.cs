using Wisdom.BuildingBlocks.Abstractions;
using Wisdom.Order.Domain.ValueObjects;
namespace Wisdom.Order.Domain.Entities;

public class PurchaseEntity : Aggregate<PurchaseId>
{
    public Guid UserId { get; private set; }
    public Guid CourseId { get; private set; }
    public decimal Price { get; private set; }

    private PurchaseEntity() { }

    public static PurchaseEntity Create(Guid userId, Guid courseId, decimal price)
    {
        return new PurchaseEntity
        {
            Id = PurchaseId.Of(Guid.NewGuid()),
            UserId = userId,
            CourseId = courseId,
            Price = price,
            CreatedAt = DateTime.UtcNow
        };
    }
}
