using Wisdom.BuildingBlocks.Exceptions;
using Wisdom.BuildingBlocks.RegularExpressions;
namespace Wisdom.User.Domain.ValueObjects;

public record Email
{
    public string Value { get; init; }

    private Email(string value) => Value = value;

    public static Email Of(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email cannot be empty.");

        return !EmailRegularExpression.IsMatch(value)
            ? throw new DomainException("Invalid email format.")
            : new Email(value);
    }
}