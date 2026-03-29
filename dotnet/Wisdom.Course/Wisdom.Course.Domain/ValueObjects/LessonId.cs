using Wisdom.BuildingBlocks.Exceptions;
namespace Wisdom.Course.Domain.ValueObjects;

public record LessonId(Guid Value)
{
    public static LessonId Of(Guid value)
    {
        if (value == Guid.Empty)
            throw new DomainException("LessonId cannot be empty.");
        return new LessonId(value);
    }
}
