using Wisdom.BuildingBlocks.Abstractions;
using Wisdom.Media.Domain.Enums;
using Wisdom.Media.Domain.ValueObjects;
namespace Wisdom.Media.Domain.Entities;

public class MediaEntity : Aggregate<MediaId>
{
    public Guid LessonId { get; private set; }
    public MediaTypeEnum Type { get; private set; }
    public MediaStatusEnum Status { get; private set; }
    public string FilePath { get; private set; } = null!;
    public string? ThumbnailPath { get; private set; }
    public int? Duration { get; private set; }

    private MediaEntity() { }

    public static MediaEntity Create(Guid lessonId, MediaTypeEnum type, string filePath, string? thumbnailPath = null)
    {
        return new MediaEntity
        {
            Id = MediaId.Of(Guid.NewGuid()),
            LessonId = lessonId,
            Type = type,
            Status = MediaStatusEnum.Uploaded,
            FilePath = filePath,
            ThumbnailPath = thumbnailPath,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateStatus(MediaStatusEnum status)
    {
        Status = status;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void SetReady(string? thumbnailPath, int? duration)
    {
        Status = MediaStatusEnum.Ready;
        ThumbnailPath = thumbnailPath;
        Duration = duration;
        LastModifiedAt = DateTime.UtcNow;
    }
}
