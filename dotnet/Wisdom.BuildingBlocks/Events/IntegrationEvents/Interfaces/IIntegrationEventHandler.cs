namespace Wisdom.BuildingBlocks.Events.IntegrationEvents.Interfaces;
public interface IIntegrationEventHandler<in TEvent> where TEvent : IIntegrationEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}