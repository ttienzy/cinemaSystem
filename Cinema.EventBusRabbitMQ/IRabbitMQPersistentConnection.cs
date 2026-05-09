using RabbitMQ.Client;

namespace Cinema.EventBusRabbitMQ;

/// <summary>
/// Manages persistent connection to RabbitMQ with automatic reconnection
/// </summary>
public interface IRabbitMQPersistentConnection : IDisposable
{
    bool IsConnected { get; }

    bool TryConnect();

    IModel CreateModel();
}
