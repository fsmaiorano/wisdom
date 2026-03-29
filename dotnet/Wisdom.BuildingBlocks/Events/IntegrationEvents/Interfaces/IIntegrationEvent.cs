namespace Wisdom.BuildingBlocks.Events.IntegrationEvents.Interfaces;

public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
    string? CorrelationId { get; }
}

