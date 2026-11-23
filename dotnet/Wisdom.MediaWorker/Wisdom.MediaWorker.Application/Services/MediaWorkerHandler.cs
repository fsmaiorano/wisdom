using Wisdom.BuildingBlocks.Events.IntegrationEvents.Contracts;
using Wisdom.BuildingBlocks.Events.IntegrationEvents.Interfaces;

namespace Wisdom.MediaWorker.Application.Services;

/// <summary>
/// Consumes <see cref="MediaReadyIntegrationEvent"/>. The event bus derives the queue name from
/// this type: wisdom.MediaReadyIntegrationEvent.MediaWorkerHandler.
/// </summary>
public class MediaWorkerHandler(MediaProcessor processor) : IIntegrationEventHandler<MediaReadyIntegrationEvent>
{
    public Task HandleAsync(MediaReadyIntegrationEvent @event, CancellationToken cancellationToken = default)
        => processor.ProcessAsync(@event, cancellationToken);
}
