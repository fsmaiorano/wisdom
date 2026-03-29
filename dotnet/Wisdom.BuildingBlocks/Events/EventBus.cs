using System.Reflection;
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

public class EventBus : IEventBus, IAsyncDisposable, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventBus> _logger;
    private readonly IConnection _connection;
    private readonly IChannel _publishChannel;
    private readonly List<IChannel> _consumerChannels = new();
    private readonly Dictionary<Type, List<Type>> _handlers = new();
    private readonly HashSet<string> _declaredExchanges = new();
    private readonly object _exchangeDeclareLock = new();
    private const string ExchangePrefix = "marketspace";

    public EventBus(IServiceProvider serviceProvider, ILogger<EventBus> logger, string rabbitMqConnectionString)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        ConnectionFactory factory = new() { Uri = new Uri(rabbitMqConnectionString) };

        var attempt = 0;
        var delay = TimeSpan.FromSeconds(2);

        while (true)
        {
            try
            {
                _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
                _publishChannel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

                _logger.LogInformation("RabbitMQ EventBus initialized");
                break;
            }
            catch (Exception ex)
            {
                attempt++;
                _logger.LogWarning(ex, "Unable to connect to RabbitMQ (attempt {Attempt}). Retrying in {Delay}s...",
                    attempt, delay.TotalSeconds);

                Thread.Sleep(delay);
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 30));
            }
        }
    }

    private static string GetExchangeName(Type eventType) => $"{ExchangePrefix}.{eventType.Name}";

    private static string GetDeadLetterExchangeName(string handlerName) => $"{ExchangePrefix}.{handlerName}_dlx";

    private async Task EnsurePublishExchangeDeclaredAsync(string exchangeName)
    {

        bool alreadyDeclared;
        lock (_exchangeDeclareLock)
        {
            alreadyDeclared = _declaredExchanges.Contains(exchangeName);
        }

        if (alreadyDeclared)
            return;

        await _publishChannel.ExchangeDeclareAsync(exchangeName, ExchangeType.Fanout, durable: true, autoDelete: false);

        lock (_exchangeDeclareLock)
        {
            _declaredExchanges.Add(exchangeName);
        }

        _logger.LogInformation("Declared exchange {ExchangeName}", exchangeName);
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IIntegrationEvent
    {
        var eventType = @event.GetType();
        var exchangeName = GetExchangeName(eventType);

        _logger.LogInformation(
            "Publishing integration event {EventType} with EventId {EventId} to exchange {ExchangeName}",
            eventType.Name, @event.EventId, exchangeName);

        await EnsurePublishExchangeDeclaredAsync(exchangeName);

        var message = JsonSerializer.Serialize(@event,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        var body = Encoding.UTF8.GetBytes(message);

        var properties = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            MessageId = @event.EventId.ToString(),
            Timestamp = new AmqpTimestamp(new DateTimeOffset(@event.OccurredAt).ToUnixTimeSeconds()),
            Headers = new Dictionary<string, object?>()
        };

        using (var scope = _serviceProvider.CreateScope())
        {
            var correlationIdService = scope.ServiceProvider.GetService<ICorrelationIdService>();
            if (correlationIdService != null)
            {
                var correlationId = correlationIdService.GetCorrelationId();
                if (!string.IsNullOrWhiteSpace(correlationId))
                {
                    properties.Headers["X-Correlation-ID"] = correlationId;
                    _logger.LogDebug("Publishing event with CorrelationId: {CorrelationId}", correlationId);
                }
            }
        }

        await _publishChannel.BasicPublishAsync(
            exchange: exchangeName,
            routingKey: string.Empty,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);

        _logger.LogDebug("Published event {EventType} to exchange {ExchangeName}", eventType.Name, exchangeName);
    }

    public void Subscribe<TEvent, THandler>()
        where TEvent : class, IIntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        SubscribeAsync<TEvent, THandler>().GetAwaiter().GetResult();
    }

    private async Task SubscribeAsync<TEvent, THandler>()
        where TEvent : class, IIntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        var eventType = typeof(TEvent);
        var handlerType = typeof(THandler);

        if (!_handlers.ContainsKey(eventType))
            _handlers[eventType] = new List<Type>();

        if (!_handlers[eventType].Contains(handlerType))
        {
            _handlers[eventType].Add(handlerType);
            _logger.LogInformation("Subscribed {HandlerType} to {EventType}", handlerType.Name, eventType.Name);
        }

        var safeHandlerName = (handlerType.FullName ?? handlerType.Name)
            .Replace('+', '.')
            .Replace('<', '_')
            .Replace('>', '_');
        var queueName = safeHandlerName;
        var deadLetterQueueName = $"{safeHandlerName}_dlq";
        var exchangeName = GetExchangeName(eventType);
        var deadLetterExchangeName = GetDeadLetterExchangeName(safeHandlerName);

        var channel = await _connection.CreateChannelAsync();
        _consumerChannels.Add(channel);

        await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Fanout, durable: true, autoDelete: false);

        await channel.ExchangeDeclareAsync(deadLetterExchangeName, ExchangeType.Fanout, durable: true,
            autoDelete: false);

        await channel.QueueDeclareAsync(
            queue: deadLetterQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        await channel.QueueBindAsync(
            queue: deadLetterQueueName,
            exchange: deadLetterExchangeName,
            routingKey: string.Empty);

        Dictionary<string, object?> queueArguments = new() { { "x-dead-letter-exchange", deadLetterExchangeName } };

        channel = await DeclareMainQueueAsync(channel, queueName, queueArguments,
            exchangeName, deadLetterExchangeName, deadLetterQueueName);

        await channel.QueueBindAsync(
            queue: queueName,
            exchange: exchangeName,
            routingKey: string.Empty);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var deliveryTag = ea.DeliveryTag;

            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                string? correlationId = null;
                if (ea.BasicProperties?.Headers?.TryGetValue("X-Correlation-ID", out var correlationIdObj) == true)
                {
                    correlationId = correlationIdObj switch
                    {
                        byte[] bytes => Encoding.UTF8.GetString(bytes),
                        string str => str,
                        _ => correlationIdObj?.ToString()
                    };
                }

                _logger.LogDebug("Received event {EventType} from RabbitMQ with CorrelationId: {CorrelationId}",
                    eventType.Name, correlationId ?? "N/A");

                _logger.LogDebug("Raw message content: {Message}", message);

                var @event = JsonSerializer.Deserialize<TEvent>(message,
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true
                    });

                if (@event != null)
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var idempotencyService =
                            scope.ServiceProvider.GetService<IIdempotencyService>();

                        if (!string.IsNullOrWhiteSpace(correlationId))
                        {
                            var correlationIdService =
                                scope.ServiceProvider.GetService<ICorrelationIdService>();
                            correlationIdService?.SetCorrelationId(correlationId);
                        }

                        using (LogContext.PushProperty("CorrelationId", correlationId ?? "N/A"))
                        {
                            if (idempotencyService != null)
                            {
                                await idempotencyService.ExecuteAsync(@event.EventId, eventType.Name,
                                    async (ct) =>
                                    {
                                        await ProcessEventAsync(@event, eventType, scope.ServiceProvider, ct);
                                    }, CancellationToken.None);
                            }
                            else
                            {
                                await ProcessEventAsync(@event, eventType, scope.ServiceProvider,
                                    CancellationToken.None);
                            }
                        }
                    }

                    await channel.BasicAckAsync(deliveryTag, false);
                    _logger.LogDebug("Message acknowledged for event {EventType}", eventType.Name);
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to deserialize event {EventType}. Rejecting message to dead letter queue.",
                        eventType.Name);

                    await channel.BasicRejectAsync(deliveryTag, false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event {EventType}. Rejecting message to dead letter queue.",
                    eventType.Name);

                await channel.BasicRejectAsync(deliveryTag, false);
            }
        };

        await channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation(
            "Started consuming queue {QueueName} bound to exchange {ExchangeName}. Dead letters → {DeadLetterExchangeName} → {DeadLetterQueueName}",
            queueName, exchangeName, deadLetterExchangeName, deadLetterQueueName);
    }

    private async Task<IChannel> DeclareMainQueueAsync(
        IChannel channel,
        string queueName,
        Dictionary<string, object?> queueArguments,
        string exchangeName,
        string deadLetterExchangeName,
        string deadLetterQueueName)
    {
        try
        {
            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: queueArguments);

            return channel;
        }
        catch (OperationInterruptedException ex) when (ex.ShutdownReason?.ReplyCode == 406)
        {

            _logger.LogWarning(
                "Queue {QueueName} exists with incompatible arguments. Recreating with updated topology...",
                queueName);

            var recoveryChannel = await _connection.CreateChannelAsync();
            _consumerChannels[^1] = recoveryChannel;

            await recoveryChannel.ExchangeDeclareAsync(exchangeName, ExchangeType.Fanout, durable: true,
                autoDelete: false);
            await recoveryChannel.ExchangeDeclareAsync(deadLetterExchangeName, ExchangeType.Fanout, durable: true,
                autoDelete: false);

            await recoveryChannel.QueueDeclareAsync(deadLetterQueueName, durable: true, exclusive: false,
                autoDelete: false);
            await recoveryChannel.QueueBindAsync(deadLetterQueueName, deadLetterExchangeName, routingKey: string.Empty);

            await recoveryChannel.QueueDeleteAsync(queueName, ifUnused: false, ifEmpty: false);
            await recoveryChannel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: queueArguments);

            _logger.LogInformation("Queue {QueueName} successfully recreated with updated arguments", queueName);
            return recoveryChannel;
        }
    }

    private async Task ProcessEventAsync<TEvent>(TEvent @event, Type eventType, IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
        where TEvent : class, IIntegrationEvent
    {
        if (!_handlers.TryGetValue(eventType, out var handlerTypes))
        {
            _logger.LogDebug("No handlers registered for event type {EventType}", eventType.Name);
            return;
        }

        var tasks = new List<Task>();

        foreach (var handlerType in handlerTypes)
        {
            var handler = serviceProvider.GetService(handlerType);
            if (handler is null)
            {
                _logger.LogWarning("Handler {HandlerType} not found in service provider", handlerType.Name);
                continue;
            }

            var method = handlerType.GetMethod("HandleAsync", new[] { eventType, typeof(CancellationToken) });
            var task = (Task?)method?.Invoke(handler, new object[] { @event, cancellationToken });

            if (task != null)
                tasks.Add(task);
        }

        if (tasks.Count > 0)
            await Task.WhenAll(tasks);
    }

    public async ValueTask DisposeAsync()
    {
        _logger.LogInformation("Disposing RabbitMQ EventBus");

        foreach (var consumerChannel in _consumerChannels)
        {
            await consumerChannel.CloseAsync();
            consumerChannel.Dispose();
        }

        await _publishChannel.CloseAsync();
        await _connection.CloseAsync();
        _publishChannel.Dispose();
        _connection.Dispose();
    }

    public void Dispose()
    {
        _logger.LogInformation("Disposing RabbitMQ EventBus");

        foreach (var consumerChannel in _consumerChannels)
            consumerChannel.Dispose();

        _publishChannel.Dispose();
        _connection.Dispose();
    }
}