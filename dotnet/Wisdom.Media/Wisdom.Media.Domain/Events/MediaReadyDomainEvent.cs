using System.Text.Json.Serialization;
using Wisdom.BuildingBlocks.Events.DomainEvents;
using Wisdom.Media.Domain.Entities;

namespace Wisdom.Media.Domain.Events;

public class MediaReadyDomainEvent : DomainEvent
{
  public string MediaType { get; init; } = string.Empty;
  public string MediaId { get; init; } = string.Empty;
  public string LessonId { get; init; } = string.Empty;
  public string FilePath { get; init; } = string.Empty;

  [JsonConstructor]
  private MediaReadyDomainEvent() { }

  private MediaReadyDomainEvent(string mediaType, string mediaId, string lessonId, string filePath, string correlationId)
    : base(correlationId)
  {
    MediaType = mediaType;
    MediaId = mediaId;
    LessonId = lessonId;
    FilePath = filePath;
  }

  public static MediaReadyDomainEvent Create(string mediaType, string mediaId, string lessonId, string filePath, string correlationId)
    => new(mediaType, mediaId, lessonId, filePath, correlationId);

  public static MediaReadyDomainEvent CreateFromEntity(MediaEntity entity, string correlationId)
    => new(
      mediaType: entity.Type.ToString(),
      mediaId: entity.Id.Value.ToString(),
      lessonId: entity.LessonId.ToString(),
      filePath: entity.FilePath,
      correlationId: correlationId
    );
}
