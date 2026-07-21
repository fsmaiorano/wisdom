using Wisdom.BuildingBlocks.Abstractions;
using Wisdom.Course.Domain.ValueObjects;

namespace Wisdom.Course.Domain.Entities;

public class LessonEntity : Aggregate<LessonId>
{
    public CourseId CourseId { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public int Position { get; private set; }
    public bool IsPreview { get; private set; }

    private LessonEntity() { }

    public static LessonEntity Create(CourseId courseId, string title, string description, int position, bool isPreview)
    {
        return new LessonEntity
        {
            Id = LessonId.Of(Guid.NewGuid()),
            CourseId = courseId,
            Title = title,
            Description = description,
            Position = position,
            IsPreview = isPreview,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string title, string description, int position, bool isPreview)
    {
        Title = title;
        Description = description;
        Position = position;
        IsPreview = isPreview;
        LastModifiedAt = DateTime.UtcNow;
    }
}
