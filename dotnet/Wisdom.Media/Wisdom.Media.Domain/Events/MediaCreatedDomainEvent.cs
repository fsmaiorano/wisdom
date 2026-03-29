using Wisdom.BuildingBlocks.Events.DomainEvents.Interfaces;

namespace Wisdom.Media.Domain.Events;

public class MediaCreatedDomainEvent : IDomainEvent
{
  public string? MediaType { get; init; }
  public string? MediaId { get; init; }
  public string? LessonId { get; init; }
  public string? FilePath { get; init; }
  public DateTime OccurredAt => DateTime.UtcNow;
}
