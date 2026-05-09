namespace Cinema.EventBus.Abstractions;

/// <summary>
/// Handler interface for processing integration events
/// </summary>
public interface IIntegrationEventHandler<in TIntegrationEvent>
    where TIntegrationEvent : IntegrationEvent
{
    Task Handle(TIntegrationEvent @event);
}

/// <summary>
/// Dynamic handler for events resolved at runtime
/// </summary>
public interface IDynamicIntegrationEventHandler
{
    Task Handle(dynamic eventData);
}
