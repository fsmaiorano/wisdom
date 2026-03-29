using Wisdom.BuildingBlocks.Events.IntegrationEvents.Interfaces;
namespace Wisdom.BuildingBlocks.Events.IntegrationEvents;

public abstract class IntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public string? CorrelationId { get; init; }
}
