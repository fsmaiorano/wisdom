using Wisdom.BuildingBlocks.Events.IntegrationEvents.Interfaces;
namespace Wisdom.BuildingBlocks.Events.Interfaces;

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IIntegrationEvent;

    /// <summary>
    /// Declares the exchange / queue topology for <typeparamref name="TEvent"/> and starts
    /// consuming messages for <typeparamref name="THandler"/>. Call once per handler at startup.
    /// </summary>
    Task SubscribeAsync<TEvent, THandler>()
        where TEvent : class, IIntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>;
}
