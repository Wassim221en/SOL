using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Template.Application.Common.Events;
using Template.Domain.Events;

namespace Template.Infrastructe.Events.RabbitMQ;

/// <summary>
/// RabbitMQ implementation of the event bus
/// </summary>
public class RabbitMQEventBus : IEventBus, IDisposable
{
    private readonly RabbitMQSettings _settings;
    private readonly ILogger<RabbitMQEventBus> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, List<Type>> _eventHandlers;
    private readonly Dictionary<string, Type> _eventTypes;
    
    private IConnection? _connection;
    private IChannel? _channel;
    private bool _disposed;

    public RabbitMQEventBus(
        IOptions<RabbitMQSettings> settings,
        ILogger<RabbitMQEventBus> logger,
        IServiceProvider serviceProvider)
    {
        _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _eventHandlers = new Dictionary<string, List<Type>>();
        _eventTypes = new Dictionary<string, Type>();
    }

    /// <summary>
    /// Publish an integration event to RabbitMQ
    /// </summary>
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) 
        where TEvent : IntegrationEvent
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

        await EnsureConnectionAsync(cancellationToken);

        var eventName = @event.GetType().Name;
        var message = JsonSerializer.Serialize(@event, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });
        var body = Encoding.UTF8.GetBytes(message);

        var properties = new BasicProperties
        {
            DeliveryMode = DeliveryModes.Persistent,
            ContentType = "application/json",
            Type = eventName,
            MessageId = @event.Id.ToString(),
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };

        try
        {
            await _channel!.BasicPublishAsync(
                exchange: _settings.ExchangeName,
                routingKey: eventName,
                mandatory: true,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Published integration event {EventId} - {EventType} to RabbitMQ",
                @event.Id,
                eventName);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error publishing integration event {EventId} - {EventType}",
                @event.Id,
                eventName);
            throw;
        }
    }

    /// <summary>
    /// Subscribe to an integration event
    /// </summary>
    public void Subscribe<TEvent, THandler>()
        where TEvent : IntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        var eventName = typeof(TEvent).Name;
        var handlerType = typeof(THandler);

        if (!_eventTypes.ContainsKey(eventName))
        {
            _eventTypes.Add(eventName, typeof(TEvent));
        }

        if (!_eventHandlers.ContainsKey(eventName))
        {
            _eventHandlers.Add(eventName, new List<Type>());
        }

        if (_eventHandlers[eventName].Contains(handlerType))
        {
            _logger.LogWarning(
                "Handler {HandlerType} already registered for event {EventName}",
                handlerType.Name,
                eventName);
            return;
        }

        _eventHandlers[eventName].Add(handlerType);

        _logger.LogInformation(
            "Subscribed to event {EventName} with handler {HandlerType}",
            eventName,
            handlerType.Name);
    }

    /// <summary>
    /// Unsubscribe from an integration event
    /// </summary>
    public void Unsubscribe<TEvent, THandler>()
        where TEvent : IntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        var eventName = typeof(TEvent).Name;
        var handlerType = typeof(THandler);

        if (_eventHandlers.ContainsKey(eventName))
        {
            _eventHandlers[eventName].Remove(handlerType);
            
            if (_eventHandlers[eventName].Count == 0)
            {
                _eventHandlers.Remove(eventName);
                _eventTypes.Remove(eventName);
            }

            _logger.LogInformation(
                "Unsubscribed from event {EventName} with handler {HandlerType}",
                eventName,
                handlerType.Name);
        }
    }

    /// <summary>
    /// Start consuming messages from RabbitMQ
    /// </summary>
    public void StartConsuming()
    {
        EnsureConnectionAsync(CancellationToken.None).GetAwaiter().GetResult();

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnMessageReceivedAsync;

        _channel!.BasicConsumeAsync(
            queue: _settings.QueueName,
            autoAck: false,
            consumer: consumer).GetAwaiter().GetResult();

        _logger.LogInformation("Started consuming messages from queue {QueueName}", _settings.QueueName);
    }

    /// <summary>
    /// Stop consuming messages from RabbitMQ
    /// </summary>
    public void StopConsuming()
    {
        if (_channel != null && _channel.IsOpen)
        {
            _channel.CloseAsync().GetAwaiter().GetResult();
            _logger.LogInformation("Stopped consuming messages");
        }
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs eventArgs)
    {
        var eventName = eventArgs.RoutingKey;
        var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

        try
        {
            await ProcessEventAsync(eventName, message);
            await _channel!.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
            
            _logger.LogInformation(
                "Processed event {EventName} with delivery tag {DeliveryTag}",
                eventName,
                eventArgs.DeliveryTag);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing event {EventName} with delivery tag {DeliveryTag}",
                eventName,
                eventArgs.DeliveryTag);

            // Reject and requeue the message for retry
            await _channel!.BasicNackAsync(
                eventArgs.DeliveryTag,
                multiple: false,
                requeue: true);
        }
    }

    private async Task ProcessEventAsync(string eventName, string message)
    {
        if (!_eventHandlers.ContainsKey(eventName))
        {
            _logger.LogWarning("No handlers registered for event {EventName}", eventName);
            return;
        }

        if (!_eventTypes.ContainsKey(eventName))
        {
            _logger.LogWarning("Event type not found for {EventName}", eventName);
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var eventType = _eventTypes[eventName];
        var integrationEvent = JsonSerializer.Deserialize(message, eventType, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        if (integrationEvent == null)
        {
            _logger.LogWarning("Failed to deserialize event {EventName}", eventName);
            return;
        }

        var handlers = _eventHandlers[eventName];
        foreach (var handlerType in handlers)
        {
            var handler = scope.ServiceProvider.GetService(handlerType);
            if (handler == null)
            {
                _logger.LogWarning("Handler {HandlerType} not found in DI container", handlerType.Name);
                continue;
            }

            var handleMethod = handlerType.GetMethod("HandleAsync");
            if (handleMethod != null)
            {
                await (Task)handleMethod.Invoke(handler, new[] { integrationEvent, CancellationToken.None })!;
            }
        }
    }

    private async Task EnsureConnectionAsync(CancellationToken cancellationToken)
    {
        if (_connection != null && _connection.IsOpen && _channel != null && _channel.IsOpen)
        {
            return;
        }

        _logger.LogInformation("Creating RabbitMQ connection to {HostName}:{Port}", _settings.HostName, _settings.Port);

        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password,
            VirtualHost = _settings.VirtualHost,
            AutomaticRecoveryEnabled = _settings.AutomaticRecoveryEnabled,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(_settings.NetworkRecoveryInterval),
            RequestedConnectionTimeout = TimeSpan.FromSeconds(_settings.ConnectionTimeout)
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        // Declare exchange
        await _channel.ExchangeDeclareAsync(
            exchange: _settings.ExchangeName,
            type: _settings.ExchangeType,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        // Declare queue
        await _channel.QueueDeclareAsync(
            queue: _settings.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        // Bind queue to exchange for all subscribed events
        foreach (var eventName in _eventHandlers.Keys)
        {
            await _channel.QueueBindAsync(
                queue: _settings.QueueName,
                exchange: _settings.ExchangeName,
                routingKey: eventName,
                arguments: null,
                cancellationToken: cancellationToken);
        }

        _logger.LogInformation("RabbitMQ connection established successfully");
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _channel?.Dispose();
        _connection?.Dispose();
        _disposed = true;

        _logger.LogInformation("RabbitMQ connection disposed");
    }
}

