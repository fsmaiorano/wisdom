namespace Wisdom.BuildingBlocks.Events.DomainEvents.Interfaces;
public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}