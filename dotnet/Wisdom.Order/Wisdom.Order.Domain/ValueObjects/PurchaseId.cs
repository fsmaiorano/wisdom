using Wisdom.BuildingBlocks.Exceptions;
namespace Wisdom.Order.Domain.ValueObjects;

public record PurchaseId(Guid Value)
{
    public static PurchaseId Of(Guid value)
    {
        if (value == Guid.Empty)
            throw new DomainException("PurchaseId cannot be empty.");
        return new PurchaseId(value);
    }
}
