using Wisdom.BuildingBlocks.Abstractions;
using Wisdom.User.Domain.Enums;
using Wisdom.User.Domain.ValueObjects;
namespace Wisdom.User.Domain.Entities;

public class UserEntity : Aggregate<UserId>
{
    public string KeycloakId { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string Password { get; private set; } = null!;
    public UserRoleEnum Role { get; private set; }
    public bool IsActive { get; private set; }

    private UserEntity() { }

    public static UserEntity Create(string keycloakId, string email, string name, string password, UserRoleEnum role, string actor)
    {
        return new UserEntity
        {
            Id = UserId.Of(Guid.NewGuid()),
            KeycloakId = keycloakId,
            Email = email,
            Name = name,
            Password = password,
            Role = role,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }

    public void Update(string email, string name, string password, UserRoleEnum role, string actor)
    {
        Email = email;
        Name = name;
        Password = password;
        Role = role;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Deactivate(string actor)
    {
        IsActive = false;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Activate(string actor)
    {
        IsActive = true;
        LastModifiedAt = DateTime.UtcNow;
    }
}
