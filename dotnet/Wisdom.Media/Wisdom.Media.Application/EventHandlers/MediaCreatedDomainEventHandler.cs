using Wisdom.BuildingBlocks.Events.DomainEvents.Interfaces;
using Wisdom.Media.Domain.Events;

namespace Wisdom.Media.Application.EventHandlers;

public class MediaCreatedDomainEventHandler : IDomainEventHandler<MediaCreatedDomainEvent>
{
  public Task HandleAsync(MediaCreatedDomainEvent @event, CancellationToken cancellationToken = default)
  {
    throw new NotImplementedException();
  }
}
