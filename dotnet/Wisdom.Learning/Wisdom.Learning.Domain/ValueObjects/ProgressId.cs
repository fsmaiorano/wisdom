using Wisdom.BuildingBlocks.Exceptions;
namespace Wisdom.Learning.Domain.ValueObjects;

public record ProgressId(Guid Value)
{
    public static ProgressId Of(Guid value)
    {
        if (value == Guid.Empty)
            throw new DomainException("ProgressId cannot be empty.");
        return new ProgressId(value);
    }
}
