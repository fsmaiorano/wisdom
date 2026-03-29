using Wisdom.BuildingBlocks.Exceptions;
namespace Wisdom.Order.Domain.ValueObjects;

public record EnrollmentId(Guid Value)
{
    public static EnrollmentId Of(Guid value)
    {
        if (value == Guid.Empty)
            throw new DomainException("EnrollmentId cannot be empty.");
        return new EnrollmentId(value);
    }
}
