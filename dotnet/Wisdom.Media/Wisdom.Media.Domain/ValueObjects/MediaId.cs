using System.Text.Json.Serialization;
using Wisdom.BuildingBlocks.Exceptions;
namespace Wisdom.Media.Domain.ValueObjects;

public record MediaId
{
    public Guid Value { get; set; }

    private MediaId(Guid value) => Value = value;

    [JsonConstructor]
    public MediaId()
    {
    }

    public static MediaId Of(Guid value)
    {
        return value == Guid.Empty
            ? throw new DomainException("MediaId cannot be empty.")
            : new MediaId(value);
    }
}
