using Wisdom.Tests.Common.Builders;
using Wisdom.Order.Domain.Entities;
namespace Wisdom.Order.UnitTests.Domain.Entities;

public class PurchaseEntityTests
{
    [Fact]
    public void Create_WithValidData_ReturnsPurchaseEntity()
    {
        var userId = BogusFactory.NewGuid();
        var courseId = BogusFactory.NewGuid();
        const decimal price = 99.99m;

        var purchase = PurchaseEntity.Create(userId, courseId, price);

        Assert.NotNull(purchase);
        Assert.NotEqual(Guid.Empty, purchase.Id.Value);
    }

    [Fact]
    public void Create_SetsCreatedAt()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);

        var purchase = PurchaseEntity.Create(BogusFactory.NewGuid(), BogusFactory.NewGuid(), 49.99m);

        Assert.NotNull(purchase.CreatedAt);
        Assert.True(purchase.CreatedAt >= before);
        Assert.True(purchase.CreatedAt <= DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void Create_StoresUserIdAndCourseId()
    {
        var userId = BogusFactory.NewGuid();
        var courseId = BogusFactory.NewGuid();
        const decimal price = 29.99m;

        var purchase = PurchaseEntity.Create(userId, courseId, price);

        Assert.Equal(userId, purchase.UserId);
        Assert.Equal(courseId, purchase.CourseId);
        Assert.Equal(price, purchase.Price);
    }
}
