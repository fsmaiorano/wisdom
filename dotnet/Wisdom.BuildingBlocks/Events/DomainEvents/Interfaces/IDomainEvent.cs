namespace Wisdom.BuildingBlocks.Events.DomainEvents.Interfaces;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}