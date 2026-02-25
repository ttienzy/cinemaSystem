using MediatR;

namespace Domain.Common
{
    /// <summary>
    /// Marker interface for all domain events.
    /// Implements INotification so MediatR can dispatch them.
    /// </summary>
    public interface IDomainEvent : INotification
    {
        Guid EventId { get; }
        DateTime OccurredAt { get; }
    }
}
