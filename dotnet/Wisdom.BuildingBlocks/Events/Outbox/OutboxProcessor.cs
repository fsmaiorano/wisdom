using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wisdom.BuildingBlocks.Events.DomainEvents.Interfaces;

namespace Wisdom.BuildingBlocks.Events.Outbox;

public class OutboxProcessor<TDbContext>(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<OutboxProcessor<TDbContext>> logger,
    IOptions<OutboxSettings> settings)
    : BackgroundService
    where TDbContext : DbContext, IOutboxDbContext
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        IncludeFields = true
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(settings.Value.IntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Unhandled error in OutboxProcessor<{DbContext}>. " +
                    "Check that all migrations have been applied (OutboxMessages / ProcessedEvents tables). " +
                    "Next retry in {Interval}s.",
                    typeof(TDbContext).Name, settings.Value.IntervalSeconds);
            }

            await Task.Delay(interval, stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var db         = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();

        var strategy = db.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            var now = DateTime.UtcNow;

            var messages = await db.OutboxMessages
                .Where(m =>
                    m.ProcessedOn == null &&
                    m.RetryCount < settings.Value.MaxRetries &&
                    (m.ScheduledAfter == null || m.ScheduledAfter <= now))
                .OrderBy(m => m.OccurredOn)
                .Take(settings.Value.BatchSize)
                .ToListAsync(ct);

            if (messages.Count == 0) return;

            logger.LogInformation("Processing {Count} outbox message(s)", messages.Count);

            foreach (var msg in messages)
            {
                await ProcessMessageAsync(msg, dispatcher, ct);
            }

            db.ChangeTracker.DetectChanges();
            await db.SaveChangesAsync(ct);
        });
    }

    private async Task ProcessMessageAsync(
        OutboxMessage msg,
        IDomainEventDispatcher dispatcher,
        CancellationToken ct)
    {
        try
        {
            logger.LogInformation(
                "Dispatching outbox message {Id} (type={Type}, attempt={Attempt})",
                msg.Id, msg.Type, msg.RetryCount + 1);

            var type = Type.GetType(msg.Type);
            if (type is null)
            {
                logger.LogError("Cannot resolve type '{Type}' for message {Id} — permanently failing", msg.Type, msg.Id);
                msg.Error = $"Cannot resolve type: {msg.Type}";
                msg.ProcessedOn = DateTime.UtcNow;
                return;
            }

            var domainEvent = JsonSerializer.Deserialize(msg.Content, type, JsonOpts);
            if (domainEvent is not IDomainEvent evt)
            {
                logger.LogError("Message {Id} did not deserialize to IDomainEvent — permanently failing", msg.Id);
                msg.Error = "Not a valid IDomainEvent";
                msg.ProcessedOn = DateTime.UtcNow;
                return;
            }

            await dispatcher.DispatchAsync(evt, ct);

            msg.ProcessedOn = DateTime.UtcNow;
            msg.Error = null;

            logger.LogInformation("Successfully processed outbox message {Id}", msg.Id);
        }
        catch (Exception ex)
        {
            msg.RetryCount++;
            msg.Error = ex.Message;

            if (msg.RetryCount >= settings.Value.MaxRetries)
            {
                // Dead-letter: mark as permanently failed.
                msg.ProcessedOn = DateTime.UtcNow;
                logger.LogError(ex,
                    "Outbox message {Id} permanently failed after {MaxRetries} attempt(s)",
                    msg.Id, settings.Value.MaxRetries);
            }
            else
            {
                // Exponential backoff: 2^retryCount × IntervalSeconds
                var backoffSeconds = Math.Pow(2, msg.RetryCount) * settings.Value.IntervalSeconds;
                msg.ScheduledAfter = DateTime.UtcNow.AddSeconds(backoffSeconds);

                logger.LogWarning(ex,
                    "Outbox message {Id} failed (attempt {Attempt}/{Max}). " +
                    "Next retry after {ScheduledAfter:O}",
                    msg.Id, msg.RetryCount, settings.Value.MaxRetries, msg.ScheduledAfter);
            }
        }
    }
}
