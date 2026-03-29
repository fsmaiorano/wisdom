using Wisdom.BuildingBlocks.Events.DomainEvents.Interfaces;

namespace Wisdom.BuildingBlocks.Events.DomainEvents;

public class DomainEvent(string correlationId) : IDomainEvent
{
  public Guid EventId { get; init; } = Guid.NewGuid();
  public DateTime OccurredAt { get; } = DateTime.UtcNow;
  public string CorrelationId { get; init; } = correlationId;
}
