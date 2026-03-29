using System.Text.Json.Serialization;
namespace Wisdom.User.Domain.ValueObjects;

public record UserId
{
    public Guid Value { get; set; }

    private UserId(Guid value) => Value = value;

    [JsonConstructor]
    public UserId()
    {
    }

    public static UserId Of(Guid value)
    {
        return value == Guid.Empty
            ? throw new ArgumentException("UserId cannot be empty.", nameof(value))
            : new UserId(value);
    }
}