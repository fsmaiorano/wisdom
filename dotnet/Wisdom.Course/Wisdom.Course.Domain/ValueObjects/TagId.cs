using Wisdom.BuildingBlocks.Exceptions;
namespace Wisdom.Course.Domain.ValueObjects;

public record TagId(Guid Value)
{
    public static TagId Of(Guid value)
    {
        if (value == Guid.Empty)
            throw new DomainException("TagId cannot be empty.");
        return new TagId(value);
    }
}
