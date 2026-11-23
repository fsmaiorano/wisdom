using Wisdom.BuildingBlocks.Events.IntegrationEvents.Interfaces;
using Wisdom.BuildingBlocks.Events.Interfaces;
namespace Wisdom.MediaWorker.IntegrationTests.Mocks;

/// <summary>Replaces the RabbitMQ event bus so tests never open a broker connection.</summary>
public class FakeEventBus : IEventBus
{
    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IIntegrationEvent => Task.CompletedTask;

    public Task SubscribeAsync<TEvent, THandler>()
        where TEvent : class, IIntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent> => Task.CompletedTask;
}
