using Wisdom.BuildingBlocks.Events.IntegrationEvents.Interfaces;
namespace Wisdom.BuildingBlocks.Events.Interfaces;

public interface IEventBus
{

    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) 
        where TEvent : class, IIntegrationEvent;
    

    void Subscribe<TEvent, THandler>()
        where TEvent : class, IIntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>;
}

