using Wisdom.BuildingBlocks.Exceptions;
using Wisdom.User.Domain.ValueObjects;
namespace Wisdom.User.UnitTests.Domain.ValueObjects;

public class UserIdTests
{
    [Fact]
    public void Of_WithValidGuid_ReturnsUserIdWithCorrectValue()
    {
        var guid   = Guid.NewGuid();
        var userId = UserId.Of(guid);

        Assert.Equal(guid, userId.Value);
    }

    [Fact]
    public void Of_WithEmptyGuid_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => UserId.Of(Guid.Empty));
        Assert.Contains("UserId cannot be empty", ex.Message);
    }

    [Fact]
    public void TwoUserIds_WithSameGuid_AreEqual()
    {
        var guid = Guid.NewGuid();
        Assert.Equal(UserId.Of(guid), UserId.Of(guid));
    }

    [Fact]
    public void TwoUserIds_WithDifferentGuids_AreNotEqual()
    {
        Assert.NotEqual(UserId.Of(Guid.NewGuid()), UserId.Of(Guid.NewGuid()));
    }

    [Fact]
    public void Value_RoundTrips_ThroughOf()
    {
        var original = Guid.NewGuid();
        var userId   = UserId.Of(original);

        Assert.Equal(original, userId.Value);
    }
}
