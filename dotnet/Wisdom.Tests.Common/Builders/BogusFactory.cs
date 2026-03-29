using Bogus;
namespace Wisdom.Tests.Common.Builders;

public static class BogusFactory
{

    public static readonly Faker Faker = new();

    public static string Email()         => Faker.Internet.Email();
    public static string FullName()      => Faker.Name.FullName();
    public static string FirstName()     => Faker.Name.FirstName();
    public static string LastName()      => Faker.Name.LastName();
    public static string StrongPassword()=> $"Test{Faker.Internet.Password(8)}1!";
    public static string Url()           => Faker.Internet.Url();
    public static Guid   NewGuid()       => Guid.NewGuid();
    public static string UniqueId()      => Guid.NewGuid().ToString("N");

    public static T Pick<T>(params T[] items) => Faker.PickRandom(items);
}
