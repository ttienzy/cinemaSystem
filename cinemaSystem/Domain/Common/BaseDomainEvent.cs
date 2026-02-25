namespace Domain.Common
{
    /// <summary>
    /// Base class for all domain events.
    /// Provides default EventId and OccurredAt.
    /// </summary>
    public abstract class BaseDomainEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }
}
