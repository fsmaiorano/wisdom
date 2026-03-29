using Wisdom.Tests.Common.Assertions;
using Wisdom.Tests.Common.Builders;
using Wisdom.User.Domain.Entities;
using Wisdom.User.Domain.Enums;
namespace Wisdom.User.UnitTests.Domain.Entities;

public class UserEntityTests
{

    [Fact]
    public void Create_WithValidParameters_ReturnsActiveUser()
    {
        var user = UserEntity.Create("kc-123", "user@test.com", "John Doe", "secret", UserRoleEnum.Customer, "system");

        Assert.NotNull(user);
        Assert.NotNull(user.Id);
        Assert.NotEqual(Guid.Empty, user.Id.Value);
        Assert.Equal("kc-123",              user.KeycloakId);
        Assert.Equal("user@test.com",       user.Email);
        Assert.Equal("John Doe",            user.Name);
        Assert.Equal("secret",              user.Password);
        Assert.Equal(UserRoleEnum.Customer, user.Role);
        Assert.True(user.IsActive);
        Assert.True(user.CreatedAt <= DateTime.UtcNow);
        Assert.Null(user.LastModifiedAt);
    }

    [Theory]
    [InlineData(UserRoleEnum.Admin)]
    [InlineData(UserRoleEnum.Teacher)]
    [InlineData(UserRoleEnum.Customer)]
    public void Create_WithAnyRole_SetsRoleCorrectly(UserRoleEnum role)
    {
        var user = UserEntity.Create("kc", "a@b.com", "Name", "pass", role, "system");
        Assert.Equal(role, user.Role);
    }

    [Fact]
    public void Create_EachCall_ProducesDifferentIds()
    {
        var u1 = FakeUserBuilder.CreateUser();
        var u2 = FakeUserBuilder.CreateUser();

        Assert.NotEqual(u1.Id.Value, u2.Id.Value);
    }

    [Fact]
    public void Create_SetsCreatedAtToNow()
    {
        var before = DateTime.UtcNow;
        var user   = FakeUserBuilder.CreateUser();
        var after  = DateTime.UtcNow;

        Assert.True(user.CreatedAt >= before && user.CreatedAt <= after);
    }

    [Fact]
    public void Deactivate_WhenActive_SetsIsActiveFalse()
    {
        var user = FakeUserBuilder.CreateUser();

        user.Deactivate("admin");

        Assert.False(user.IsActive);
    }

    [Fact]
    public void Deactivate_WhenActive_SetsLastModifiedAt()
    {
        var user = FakeUserBuilder.CreateUser();

        user.Deactivate("admin");

        Assert.NotNull(user.LastModifiedAt);
        Assert.True(user.LastModifiedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Deactivate_CalledTwice_RemainsInactive()
    {
        var user = FakeUserBuilder.CreateUser();
        user.Deactivate("admin");
        user.Deactivate("admin");

        Assert.False(user.IsActive);
    }

    [Fact]
    public void Activate_WhenInactive_SetsIsActiveTrue()
    {
        var user = FakeUserBuilder.CreateUser();
        user.Deactivate("admin");

        user.Activate("admin");

        Assert.True(user.IsActive);
    }

    [Fact]
    public void Activate_WhenInactive_SetsLastModifiedAt()
    {
        var user = FakeUserBuilder.CreateUser();
        user.Deactivate("admin");
        var beforeActivate = DateTime.UtcNow;

        user.Activate("admin");

        Assert.NotNull(user.LastModifiedAt);
        Assert.True(user.LastModifiedAt >= beforeActivate);
    }

    [Fact]
    public void Activate_WhenAlreadyActive_RemainsActive()
    {
        var user = FakeUserBuilder.CreateUser();
        user.Activate("admin");

        Assert.True(user.IsActive);
    }

    [Fact]
    public void Activate_AfterDeactivate_IsActiveIsTrue()
    {
        var user = FakeUserBuilder.CreateUser();
        user.Deactivate("admin");
        user.Activate("admin");
        user.Deactivate("admin");
        user.Activate("admin");

        Assert.True(user.IsActive);
    }

    [Fact]
    public void Update_ChangesEmail()
    {
        var user = FakeUserBuilder.CreateUser(email: "old@test.com");
        user.Update("new@test.com", user.Name, user.Password, user.Role, "admin");
        Assert.Equal("new@test.com", user.Email);
    }

    [Fact]
    public void Update_ChangesName()
    {
        var user = FakeUserBuilder.CreateUser(name: "Old Name");
        user.Update(user.Email, "New Name", user.Password, user.Role, "admin");
        Assert.Equal("New Name", user.Name);
    }

    [Fact]
    public void Update_ChangesPassword()
    {
        var user = FakeUserBuilder.CreateUser();
        user.Update(user.Email, user.Name, "newpass", user.Role, "admin");
        Assert.Equal("newpass", user.Password);
    }

    [Fact]
    public void Update_ChangesRole()
    {
        var user = FakeUserBuilder.CreateUser(role: UserRoleEnum.Customer);
        user.Update(user.Email, user.Name, user.Password, UserRoleEnum.Admin, "admin");
        Assert.Equal(UserRoleEnum.Admin, user.Role);
    }

    [Fact]
    public void Update_SetsLastModifiedAt()
    {
        var user   = FakeUserBuilder.CreateUser();
        var before = DateTime.UtcNow;

        user.Update(user.Email, user.Name, user.Password, user.Role, "admin");

        Assert.NotNull(user.LastModifiedAt);
        Assert.True(user.LastModifiedAt >= before);
    }

    [Fact]
    public void Update_DoesNotChangeKeycloakId()
    {
        var keycloakId = Guid.NewGuid().ToString();
        var user       = FakeUserBuilder.CreateUser(keycloakId: keycloakId);

        user.Update("x@y.com", "X", "p", UserRoleEnum.Teacher, "admin");

        Assert.Equal(keycloakId, user.KeycloakId);
    }

    [Fact]
    public void Update_DoesNotChangeIsActive()
    {
        var user = FakeUserBuilder.CreateUser();
        Assert.True(user.IsActive);

        user.Update(user.Email, user.Name, user.Password, user.Role, "admin");

        Assert.True(user.IsActive);
    }
}
