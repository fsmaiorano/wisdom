using Wisdom.BuildingBlocks.Abstractions;
using Wisdom.Learning.Domain.ValueObjects;
namespace Wisdom.Learning.Domain.Entities;

public class ProgressEntity : Aggregate<ProgressId>
{
    public Guid UserId { get; private set; }
    public Guid LessonId { get; private set; }
    public int WatchedSeconds { get; private set; }
    public bool IsCompleted { get; private set; }

    private ProgressEntity() { }

    public static ProgressEntity Create(Guid userId, Guid lessonId)
    {
        return new ProgressEntity
        {
            Id = ProgressId.Of(Guid.NewGuid()),
            UserId = userId,
            LessonId = lessonId,
            WatchedSeconds = 0,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(int watchedSeconds, bool isCompleted)
    {
        WatchedSeconds = watchedSeconds;
        IsCompleted = isCompleted;
        LastModifiedAt = DateTime.UtcNow;
    }
}
