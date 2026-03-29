using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
namespace Wisdom.BuildingBlocks.Events.Idempotency;

public class IdempotencyService<TContext> : IIdempotencyService where TContext : DbContext
{
    private readonly TContext _context;

    public IdempotencyService(TContext context)
    {
        _context = context;
    }

    public async Task ExecuteAsync(Guid eventId, string eventName, Func<CancellationToken, Task> handler, CancellationToken cancellationToken = default)
    {

        var strategy = _context.Database.CreateExecutionStrategy();
        
        await strategy.ExecuteAsync(async () =>
        {
            _context.ChangeTracker.Clear();

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            
            var exists = await _context.Set<ProcessedEvent>()
                .AnyAsync(pe => pe.EventId == eventId, cancellationToken);

            if (exists)
            {

                await transaction.RollbackAsync(cancellationToken);
                return;
            }

            await handler(cancellationToken);

            var processedEvent = new ProcessedEvent
            {
                EventId = eventId,
                ProcessedAt = DateTime.UtcNow,
                EventType = eventName
            };

            await _context.Set<ProcessedEvent>().AddAsync(processedEvent, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        });
    }
}

