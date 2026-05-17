using Cinema.Messaging.Abstractions;
using MassTransit;

namespace Cinema.Messaging.Infrastructure;

/// <summary>
/// MassTransit implementation of IEventBus.
/// Wraps IPublishEndpoint to keep the Application layer framework-agnostic.
/// </summary>
internal sealed class MassTransitEventBus : IEventBus
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitEventBus(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
    }

    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        return _publishEndpoint.Publish(message, cancellationToken);
    }
}
