using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Wisdom.BuildingBlocks.Events.DomainEvents.Interfaces;
namespace Wisdom.BuildingBlocks.Events.DomainEvents;

public class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var eventType = domainEvent.GetType();
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);

        using var scope = serviceProvider.CreateScope();
        var handlers = scope.ServiceProvider.GetServices(handlerType);

        List<Task> tasks = [];
        foreach (var handler in handlers)
        {
            var method = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync));
            var task = (Task?)method?.Invoke(handler, [domainEvent, cancellationToken]);
            if (task != null)
                tasks.Add(task);
        }

        if (tasks.Count > 0)
            await Task.WhenAll(tasks);
    }

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
            await DispatchAsync(domainEvent, cancellationToken);
    }
}