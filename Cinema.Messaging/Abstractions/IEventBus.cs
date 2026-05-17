namespace Cinema.Messaging.Abstractions;

/// <summary>
/// Transport-agnostic abstraction for publishing integration events.
/// Keeps Application layer decoupled from MassTransit infrastructure.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publishes a message to all subscribed consumers.
    /// </summary>
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class;
}
