using Wisdom.BuildingBlocks.Abstractions;
using Wisdom.Course.Domain.ValueObjects;
namespace Wisdom.Course.Domain.Entities;

public class CourseEntity : Aggregate<CourseId>
{
    public Guid TeacherId { get; private set; }
    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public decimal Price { get; private set; }
    public bool IsPublished { get; private set; }

    private CourseEntity() { }

    public static CourseEntity Create(Guid teacherId, string title, string description, decimal price)
    {
        return new CourseEntity
        {
            Id = CourseId.Of(Guid.NewGuid()),
            TeacherId = teacherId,
            Title = title,
            Description = description,
            Price = price,
            IsPublished = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string title, string description, decimal price)
    {
        Title = title;
        Description = description;
        Price = price;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Publish()
    {
        IsPublished = true;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Unpublish()
    {
        IsPublished = false;
        LastModifiedAt = DateTime.UtcNow;
    }
}
