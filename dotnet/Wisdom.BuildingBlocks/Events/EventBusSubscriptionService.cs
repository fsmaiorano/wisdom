using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Wisdom.BuildingBlocks.Events.IntegrationEvents.Interfaces;
using Wisdom.BuildingBlocks.Events.Interfaces;

namespace Wisdom.BuildingBlocks.Events;

/// <summary>
/// Hosted service that wires a single integration event handler to the event bus
/// at application startup. Register one instance per handler:
/// <code>
///   services.AddHostedService&lt;EventBusSubscriptionService&lt;MyEvent, MyHandler&gt;&gt;();
/// </code>
/// </summary>
public sealed class EventBusSubscriptionService<TEvent, THandler>(
    IEventBus eventBus,
    ILogger<EventBusSubscriptionService<TEvent, THandler>> logger)
    : IHostedService
    where TEvent : class, IIntegrationEvent
    where THandler : IIntegrationEventHandler<TEvent>
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Registering subscription: {Handler} → {Event}",
            typeof(THandler).Name, typeof(TEvent).Name);

        await eventBus.SubscribeAsync<TEvent, THandler>();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
