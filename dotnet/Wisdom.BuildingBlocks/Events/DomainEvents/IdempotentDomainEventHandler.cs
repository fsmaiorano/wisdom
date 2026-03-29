using Wisdom.BuildingBlocks.Events.DomainEvents.Interfaces;
using Wisdom.BuildingBlocks.Events.Idempotency;

namespace Wisdom.BuildingBlocks.Events.DomainEvents;

public abstract class IdempotentDomainEventHandler<TEvent>(IIdempotencyService idempotencyService)
    : IDomainEventHandler<TEvent>
    where TEvent : IDomainEvent
{
    public Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default)
        => idempotencyService.ExecuteAsync(
            @event.EventId,
            typeof(TEvent).Name,
            ct => HandleIdempotentAsync(@event, ct),
            cancellationToken);

    protected abstract Task HandleIdempotentAsync(TEvent @event, CancellationToken cancellationToken);
}
