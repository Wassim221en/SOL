namespace Template.Infrastructe.Events.RabbitMQ;

/// <summary>
/// Configuration settings for RabbitMQ connection
/// </summary>
public class RabbitMQSettings
{
    public const string SectionName = "RabbitMQ";

    /// <summary>
    /// RabbitMQ host address
    /// </summary>
    public string HostName { get; set; } = "72.62.186.105";

    /// <summary>
    /// RabbitMQ port
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// RabbitMQ username
    /// </summary>
    public string UserName { get; set; } = "appuser";

    /// <summary>
    /// RabbitMQ password
    /// </summary>
    public string Password { get; set; } = "wpify@14#7gd6!WZ";

    /// <summary>
    /// Virtual host
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// Exchange name for integration events
    /// </summary>
    public string ExchangeName { get; set; } = "template_integration_events";

    /// <summary>
    /// Exchange type (direct, topic, fanout, headers)
    /// </summary>
    public string ExchangeType { get; set; } = "topic";

    /// <summary>
    /// Queue name for this service
    /// </summary>
    public string QueueName { get; set; } = "template_api_queue";

    /// <summary>
    /// Number of retry attempts for failed messages
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Connection timeout in seconds
    /// </summary>
    public int ConnectionTimeout { get; set; } = 30;

    /// <summary>
    /// Enable automatic recovery
    /// </summary>
    public bool AutomaticRecoveryEnabled { get; set; } = true;

    /// <summary>
    /// Network recovery interval in seconds
    /// </summary>
    public int NetworkRecoveryInterval { get; set; } = 5;
}

