using Wisdom.BuildingBlocks.Abstractions;
using Wisdom.Course.Domain.ValueObjects;
namespace Wisdom.Course.Domain.Entities;

public class TagEntity : Entity<TagId>
{
    public string Name { get; private set; } = null!;

    private TagEntity() { }

    public static TagEntity Create(string name)
    {
        return new TagEntity
        {
            Id = TagId.Of(Guid.NewGuid()),
            Name = name,
            CreatedAt = DateTime.UtcNow
        };
    }
}
