using Wisdom.BuildingBlocks.Events.DomainEvents.Interfaces;

namespace Wisdom.BuildingBlocks.Events.DomainEvents;

public class DomainEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = string.Empty;

    // Parameterless constructor required for JSON deserialization by OutboxProcessor.
    protected DomainEvent() { }

    public DomainEvent(string correlationId)
    {
        CorrelationId = correlationId;
    }
}
