using Wisdom.BuildingBlocks.Abstractions;
using Wisdom.Order.Domain.ValueObjects;
namespace Wisdom.Order.Domain.Entities;

public class EnrollmentEntity : Aggregate<EnrollmentId>
{
    public Guid UserId { get; private set; }
    public Guid CourseId { get; private set; }

    private EnrollmentEntity() { }

    public static EnrollmentEntity Create(Guid userId, Guid courseId)
    {
        return new EnrollmentEntity
        {
            Id = EnrollmentId.Of(Guid.NewGuid()),
            UserId = userId,
            CourseId = courseId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
