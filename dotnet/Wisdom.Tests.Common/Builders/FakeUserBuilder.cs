using Bogus;
using Wisdom.User.Application.Dtos;
using Wisdom.User.Domain.Entities;
using Wisdom.User.Domain.Enums;
namespace Wisdom.Tests.Common.Builders;

public static class FakeUserBuilder
{
    private static readonly Faker Faker = new();

    public static UserEntity CreateUser(
        string?       email      = null,
        string?       name       = null,
        string?       keycloakId = null,
        UserRoleEnum  role       = UserRoleEnum.Customer,
        string        actor      = "test")
        => UserEntity.Create(
            keycloakId ?? Guid.NewGuid().ToString(),
            email      ?? Faker.Internet.Email(),
            name       ?? Faker.Name.FullName(),
            BogusFactory.StrongPassword(),
            role,
            actor);

    public static RegisterUserRequest CreateRegisterRequest(
        string?      email    = null,
        string?      name     = null,
        string?      password = null,
        UserRoleEnum role     = UserRoleEnum.Customer)
        => new(
            email    ?? Faker.Internet.Email(),
            name     ?? Faker.Name.FullName(),
            password ?? BogusFactory.StrongPassword(),
            role);

    public static LoginUserRequest CreateLoginRequest(
        string email,
        string password = "TestPass123!")
        => new(email, password);

    public static Faker<RegisterUserRequest> RegisterRequestFaker() =>
        new Faker<RegisterUserRequest>()
            .CustomInstantiator(f => new RegisterUserRequest(
                f.Internet.Email(),
                f.Name.FullName(),
                BogusFactory.StrongPassword(),
                UserRoleEnum.Customer));
}
