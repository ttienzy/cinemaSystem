namespace Cinema.Contracts.Events;

/// <summary>
/// Base record for all integration events in the Cinema system.
/// Provides common metadata: EventId, CorrelationId, OccurredAt.
/// MassTransit auto-detects CorrelationId by convention — no need for CorrelatedBy interface.
/// </summary>
public abstract record IntegrationEvent
{
    /// <summary>
    /// Unique identifier for this specific event instance.
    /// </summary>
    public Guid EventId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Correlation identifier used to trace related messages across services.
    /// MassTransit automatically uses this property via convention-based correlation.
    /// </summary>
    public Guid CorrelationId { get; init; }

    /// <summary>
    /// UTC timestamp when this event was created.
    /// </summary>
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
