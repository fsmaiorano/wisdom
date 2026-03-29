using Wisdom.Tests.Common.Builders;
using Wisdom.Order.Domain.Entities;
namespace Wisdom.Order.UnitTests.Domain.Entities;

public class EnrollmentEntityTests
{
    [Fact]
    public void Create_WithValidData_ReturnsEnrollmentEntity()
    {
        var userId = BogusFactory.NewGuid();
        var courseId = BogusFactory.NewGuid();

        var enrollment = EnrollmentEntity.Create(userId, courseId);

        Assert.NotNull(enrollment);
        Assert.NotEqual(Guid.Empty, enrollment.Id.Value);
        Assert.Equal(userId, enrollment.UserId);
        Assert.Equal(courseId, enrollment.CourseId);
        Assert.NotNull(enrollment.CreatedAt);
    }
}
