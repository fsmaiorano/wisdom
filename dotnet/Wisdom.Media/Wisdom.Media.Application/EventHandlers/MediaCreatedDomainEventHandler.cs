using Microsoft.Extensions.Logging;
using Wisdom.BuildingBlocks.Events.DomainEvents;
using Wisdom.BuildingBlocks.Events.Idempotency;
using Wisdom.Media.Domain.Events;

namespace Wisdom.Media.Application.EventHandlers;

public class MediaCreatedDomainEventHandler(
    ILogger<MediaCreatedDomainEventHandler> logger,
    IIdempotencyService idempotencyService)
    : IdempotentDomainEventHandler<MediaCreatedDomainEvent>(idempotencyService)
{
    protected override Task HandleIdempotentAsync(MediaCreatedDomainEvent @event, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Handling MediaCreatedDomainEvent: MediaId={MediaId}, LessonId={LessonId}, MediaType={MediaType}, CorrelationId={CorrelationId}",
            @event.MediaId, @event.LessonId, @event.MediaType, @event.CorrelationId);

        return Task.CompletedTask;
    }
}
