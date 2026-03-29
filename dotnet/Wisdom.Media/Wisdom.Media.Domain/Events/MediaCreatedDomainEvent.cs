using Wisdom.BuildingBlocks.Events.DomainEvents;
using Wisdom.Media.Domain.Entities;

namespace Wisdom.Media.Domain.Events;

public class MediaCreatedDomainEvent : DomainEvent
{
  public string MediaType { get; init; }
  public string MediaId { get; init; }
  public string LessonId { get; init; }
  public string FilePath { get; init; }

  private MediaCreatedDomainEvent(string mediaType, string mediaId, string lessonId, string filePath, string correlationId)
    : base(correlationId)
  {
    MediaType = mediaType;
    MediaId = mediaId;
    LessonId = lessonId;
    FilePath = filePath;
  }

  public static MediaCreatedDomainEvent Create(string mediaType, string mediaId, string lessonId, string filePath, string correlationId)
    => new(mediaType, mediaId, lessonId, filePath, correlationId);

  public static MediaCreatedDomainEvent CreateFromEntity(MediaEntity entity, string correlationId)
    => new(
      mediaType: entity.Type.ToString(),
      mediaId: entity.Id.Value.ToString(),
      lessonId: entity.LessonId.ToString(),
      filePath: entity.FilePath,
      correlationId: correlationId
    );
}
