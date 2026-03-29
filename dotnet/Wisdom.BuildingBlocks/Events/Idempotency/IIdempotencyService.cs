namespace Wisdom.BuildingBlocks.Events.Idempotency;
public interface IIdempotencyService
{

    Task ExecuteAsync(Guid eventId, string eventName, Func<CancellationToken, Task> handler, CancellationToken cancellationToken = default);
}

