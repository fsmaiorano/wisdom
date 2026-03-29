using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Serilog.Context;
using Wisdom.BuildingBlocks.Events.Idempotency;
using Wisdom.BuildingBlocks.Events.IntegrationEvents.Interfaces;
using Wisdom.BuildingBlocks.Events.Interfaces;
using Wisdom.BuildingBlocks.Services.Correlation;

namespace Wisdom.BuildingBlocks.Events;

/// <summary>
/// RabbitMQ-backed event bus.
///
/// Topology (all durable):
///   Exchange  : wisdom.{EventTypeName}                        — fanout
///   Queue     : wisdom.{EventTypeName}.{HandlerTypeName}      — binds to exchange above
///   DLX       : wisdom.{EventTypeName}.{HandlerTypeName}.dlx  — fanout (dead-letter)
///   DLQ       : wisdom.{EventTypeName}.{HandlerTypeName}.dlq  — receives rejected messages (TTL configured)
///
/// Publishing uses per-operation channels with Publisher Confirms to guarantee
/// at-least-once delivery from the publisher's side.
/// Consuming uses QoS prefetch to bound in-flight message count per consumer.
/// </summary>
public sealed class EventBus : IEventBus, IAsyncDisposable, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventBus> _logger;
    private readonly IConnection _connection;
    private readonly EventBusOptions _options;
    private readonly List<IChannel> _consumerChannels = [];
    private readonly Dictionary<Type, List<Type>> _handlers = [];
    private bool _disposed;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public EventBus(
        IServiceProvider serviceProvider,
        ILogger<EventBus> logger,
        string connectionString,
        EventBusOptions? options = null)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options ?? new EventBusOptions();

        var factory = new ConnectionFactory { Uri = new Uri(connectionString) };

        var attempt = 0;
        var delay = TimeSpan.FromSeconds(2);

        while (true)
        {
            try
            {
                _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
                _logger.LogInformation("RabbitMQ EventBus connected");
                break;
            }
            catch (Exception ex)
            {
                attempt++;
                _logger.LogWarning(ex,
                    "Unable to connect to RabbitMQ (attempt {Attempt}). Retrying in {Delay}s…",
                    attempt, delay.TotalSeconds);

                Thread.Sleep(delay);
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 30));
            }
        }
    }

    // ── Naming helpers ───────────────────────────────────────────────────────

    private string ExchangeName(Type eventType) =>
        $"{_options.Prefix}.{eventType.Name}";

    private string QueueName(Type eventType, Type handlerType) =>
        $"{_options.Prefix}.{eventType.Name}.{handlerType.Name}";

    private string DlxName(Type eventType, Type handlerType) =>
        $"{_options.Prefix}.{eventType.Name}.{handlerType.Name}.dlx";

    private string DlqName(Type eventType, Type handlerType) =>
        $"{_options.Prefix}.{eventType.Name}.{handlerType.Name}.dlq";

    // ── Publish (per-operation channel + Publisher Confirms) ─────────────────

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IIntegrationEvent
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var eventType = @event.GetType();
        var exchange = ExchangeName(eventType);

        // Each publish gets its own channel in confirm mode — no shared mutable state,
        // no concurrency hazards, and confirm tracking is channel-scoped.
        await using var channel = await _connection.CreateChannelAsync(
            new CreateChannelOptions(
                publisherConfirmationsEnabled: true,
                publisherConfirmationTrackingEnabled: true),
            cancellationToken);

        // Declare is idempotent; safe to call on every publish for exchange self-healing.
        await channel.ExchangeDeclareAsync(
            exchange, ExchangeType.Fanout, durable: true, autoDelete: false,
            cancellationToken: cancellationToken);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event, eventType, JsonOpts));

        var props = new BasicProperties
        {
            ContentType    = "application/json",
            DeliveryMode   = DeliveryModes.Persistent,
            MessageId      = @event.EventId.ToString(),
            Timestamp      = new AmqpTimestamp(new DateTimeOffset(@event.OccurredAt).ToUnixTimeSeconds()),
            Headers        = new Dictionary<string, object?>()
        };

        // Propagate correlation ID if available.
        using (var scope = _serviceProvider.CreateScope())
        {
            var correlationService = scope.ServiceProvider.GetService<ICorrelationIdService>();
            var cid = correlationService?.GetCorrelationId();
            if (!string.IsNullOrWhiteSpace(cid))
                props.Headers["X-Correlation-ID"] = cid;
        }

        _logger.LogInformation(
            "Publishing {EventType} (EventId={EventId}) → exchange '{Exchange}'",
            eventType.Name, @event.EventId, exchange);

        // With PublisherConfirmationTrackingEnabled = true, this awaitable completes only when
        // the broker acks the message. It throws on nack or connection loss — the OutboxProcessor
        // will retry the outbox message on the next cycle.
        await channel.BasicPublishAsync(
            exchange: exchange,
            routingKey: string.Empty,
            mandatory: false,
            basicProperties: props,
            body: body,
            cancellationToken: cancellationToken);

        _logger.LogDebug("Confirmed delivery of {EventType} (EventId={EventId})",
            eventType.Name, @event.EventId);
    }

    // ── Subscribe ────────────────────────────────────────────────────────────

    public async Task SubscribeAsync<TEvent, THandler>()
        where TEvent : class, IIntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var eventType   = typeof(TEvent);
        var handlerType = typeof(THandler);

        if (!_handlers.ContainsKey(eventType))
            _handlers[eventType] = [];

        if (!_handlers[eventType].Contains(handlerType))
            _handlers[eventType].Add(handlerType);

        var exchange = ExchangeName(eventType);
        var queue    = QueueName(eventType, handlerType);
        var dlx      = DlxName(eventType, handlerType);
        var dlq      = DlqName(eventType, handlerType);

        var channel = await _connection.CreateChannelAsync();
        _consumerChannels.Add(channel);

        // QoS — limit unacknowledged messages per consumer channel.
        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: _options.PrefetchCount,
            global: false);

        // Main exchange.
        await channel.ExchangeDeclareAsync(
            exchange, ExchangeType.Fanout, durable: true, autoDelete: false);

        // Dead-letter exchange.
        await channel.ExchangeDeclareAsync(
            dlx, ExchangeType.Fanout, durable: true, autoDelete: false);

        // Dead-letter queue with TTL.
        await channel.QueueDeclareAsync(
            queue: dlq,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?> { { "x-message-ttl", _options.DeadLetterTtlMs } });

        await channel.QueueBindAsync(dlq, dlx, routingKey: string.Empty);

        // Main queue — rejects route to DLX.
        channel = await DeclareMainQueueAsync(channel, queue, exchange, dlx, dlq);

        await channel.QueueBindAsync(queue, exchange, routingKey: string.Empty);

        // Consumer.
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (_, ea) =>
            HandleMessageAsync<TEvent, THandler>(channel, ea, eventType, handlerType);

        await channel.BasicConsumeAsync(queue: queue, autoAck: false, consumer: consumer);

        _logger.LogInformation(
            "Subscribed {Handler} → queue '{Queue}' (exchange '{Exchange}'). DLQ: '{Dlq}'",
            handlerType.Name, queue, exchange, dlq);
    }

    // ── Message handling ─────────────────────────────────────────────────────

    private async Task HandleMessageAsync<TEvent, THandler>(
        IChannel channel,
        BasicDeliverEventArgs ea,
        Type eventType,
        Type handlerType)
        where TEvent : class, IIntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        var deliveryTag = ea.DeliveryTag;

        try
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());
            var correlationId = ExtractCorrelationId(ea);

            _logger.LogDebug("Received {EventType} (CorrelationId={CorrelationId})",
                eventType.Name, correlationId ?? "N/A");

            var @event = JsonSerializer.Deserialize<TEvent>(message, JsonOpts);

            if (@event is null)
            {
                _logger.LogWarning("Deserialization failed for {EventType} — rejecting to DLQ", eventType.Name);
                await channel.BasicRejectAsync(deliveryTag, requeue: false);
                return;
            }

            using var scope = _serviceProvider.CreateScope();

            // Propagate correlation ID into the request context.
            if (!string.IsNullOrWhiteSpace(correlationId))
                scope.ServiceProvider.GetService<ICorrelationIdService>()?.SetCorrelationId(correlationId);

            using (LogContext.PushProperty("CorrelationId", correlationId ?? "N/A"))
            {
                var idempotency = scope.ServiceProvider.GetService<IIdempotencyService>();

                if (idempotency is not null)
                {
                    await idempotency.ExecuteAsync(
                        @event.EventId,
                        eventType.Name,
                        ct => InvokeHandlerAsync(@event, eventType, handlerType, scope.ServiceProvider, ct),
                        CancellationToken.None);
                }
                else
                {
                    await InvokeHandlerAsync(@event, eventType, handlerType, scope.ServiceProvider, CancellationToken.None);
                }
            }

            await channel.BasicAckAsync(deliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing {EventType} (delivery tag {DeliveryTag}) — rejecting to DLQ",
                eventType.Name, deliveryTag);

            await channel.BasicRejectAsync(deliveryTag, requeue: false);
        }
    }

    private static async Task InvokeHandlerAsync(
        object @event,
        Type eventType,
        Type handlerType,
        IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        var handler = serviceProvider.GetService(handlerType);
        if (handler is null) return;

        var method = handlerType.GetMethod("HandleAsync", [eventType, typeof(CancellationToken)]);
        var task = (Task?)method?.Invoke(handler, [@event, ct]);

        if (task is not null)
            await task;
    }

    private static string? ExtractCorrelationId(BasicDeliverEventArgs ea)
    {
        if (ea.BasicProperties?.Headers?.TryGetValue("X-Correlation-ID", out var raw) != true)
            return null;

        return raw switch
        {
            byte[] bytes => Encoding.UTF8.GetString(bytes),
            string str   => str,
            _            => raw?.ToString()
        };
    }

    // ── Queue topology helpers ───────────────────────────────────────────────

    private async Task<IChannel> DeclareMainQueueAsync(
        IChannel channel,
        string queue,
        string exchange,
        string dlx,
        string dlq)
    {
        var args = new Dictionary<string, object?> { { "x-dead-letter-exchange", dlx } };

        try
        {
            await channel.QueueDeclareAsync(
                queue: queue, durable: true, exclusive: false, autoDelete: false, arguments: args);

            return channel;
        }
        catch (OperationInterruptedException ex) when (ex.ShutdownReason?.ReplyCode == 406)
        {
            // Queue exists with incompatible arguments — recreate on a fresh channel.
            _logger.LogWarning(
                "Queue '{Queue}' has incompatible arguments (406). Recreating…", queue);

            var fresh = await _connection.CreateChannelAsync();
            _consumerChannels[^1] = fresh;

            await fresh.ExchangeDeclareAsync(exchange, ExchangeType.Fanout, durable: true, autoDelete: false);
            await fresh.ExchangeDeclareAsync(dlx, ExchangeType.Fanout, durable: true, autoDelete: false);
            await fresh.QueueDeclareAsync(dlq, durable: true, exclusive: false, autoDelete: false,
                arguments: new Dictionary<string, object?> { { "x-message-ttl", _options.DeadLetterTtlMs } });
            await fresh.QueueBindAsync(dlq, dlx, routingKey: string.Empty);

            await fresh.QueueDeleteAsync(queue, ifUnused: false, ifEmpty: false);
            await fresh.QueueDeclareAsync(
                queue: queue, durable: true, exclusive: false, autoDelete: false, arguments: args);

            _logger.LogInformation("Queue '{Queue}' recreated with updated topology", queue);
            return fresh;
        }
    }

    // ── Disposal ─────────────────────────────────────────────────────────────

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        _logger.LogInformation("Disposing RabbitMQ EventBus");

        foreach (var ch in _consumerChannels)
        {
            try { await ch.CloseAsync(); } catch { /* best-effort */ }
            ch.Dispose();
        }

        try { await _connection.CloseAsync(); } catch { /* best-effort */ }
        _connection.Dispose();
    }

    public void Dispose() =>
        DisposeAsync().AsTask().GetAwaiter().GetResult();
}
