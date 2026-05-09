namespace Cinema.EventBus.Abstractions;

/// <summary>
/// Event bus abstraction for publish/subscribe messaging
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publish an integration event to all subscribers
    /// </summary>
    void Publish(IntegrationEvent @event);

    /// <summary>
    /// Subscribe to an integration event with a strongly-typed handler
    /// </summary>
    void Subscribe<T, TH>()
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler<T>;

    /// <summary>
    /// Subscribe to an event using dynamic handler (resolved at runtime)
    /// </summary>
    void SubscribeDynamic<TH>(string eventName)
        where TH : IDynamicIntegrationEventHandler;

    /// <summary>
    /// Unsubscribe from an integration event
    /// </summary>
    void Unsubscribe<T, TH>()
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler<T>;

    /// <summary>
    /// Unsubscribe from a dynamic event handler
    /// </summary>
    void UnsubscribeDynamic<TH>(string eventName)
        where TH : IDynamicIntegrationEventHandler;
}
