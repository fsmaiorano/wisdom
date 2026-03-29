using Wisdom.BuildingBlocks.Exceptions;
namespace Wisdom.Course.Domain.ValueObjects;

public record CourseId(Guid Value)
{
    public static CourseId Of(Guid value)
    {
        if (value == Guid.Empty)
            throw new DomainException("CourseId cannot be empty.");
        return new CourseId(value);
    }
}
