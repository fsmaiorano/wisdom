using Microsoft.Extensions.Logging;
using Wisdom.BuildingBlocks.Events.DomainEvents;
using Wisdom.BuildingBlocks.Events.Idempotency;
using Wisdom.BuildingBlocks.Events.IntegrationEvents.Contracts;
using Wisdom.BuildingBlocks.Events.Interfaces;
using Wisdom.Media.Domain.Events;

namespace Wisdom.Media.Application.EventHandlers;

public class MediaReadyDomainEventHandler(
    ILogger<MediaReadyDomainEventHandler> logger,
    IIdempotencyService idempotencyService,
    IEventBus eventBus)
    : IdempotentDomainEventHandler<MediaReadyDomainEvent>(idempotencyService)
{
    protected override async Task HandleIdempotentAsync(
        MediaReadyDomainEvent @event,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Handling MediaReadyDomainEvent: MediaId={MediaId}, LessonId={LessonId}, " +
            "MediaType={MediaType}, CorrelationId={CorrelationId}",
            @event.MediaId, @event.LessonId, @event.MediaType, @event.CorrelationId);

        var integrationEvent = new MediaReadyIntegrationEvent
        {
            MediaId       = @event.MediaId,
            LessonId      = @event.LessonId,
            MediaType     = @event.MediaType,
            FilePath      = @event.FilePath,
            CorrelationId = @event.CorrelationId
        };

        await eventBus.PublishAsync(integrationEvent, cancellationToken);

        logger.LogInformation(
            "Published MediaReadyIntegrationEvent for MediaId={MediaId}", @event.MediaId);
    }
}
