namespace Domain.Common
{
    /// <summary>
    /// Base class for all domain entities.
    /// Provides Id, and Domain Event raising/clearing support.
    /// </summary>
    public abstract class BaseEntity
    {
        public Guid Id { get; private set; }

        // Domain Events
        private readonly List<IDomainEvent> _domainEvents = [];

        public IReadOnlyCollection<IDomainEvent> DomainEvents
            => _domainEvents.AsReadOnly();

        protected void Raise(IDomainEvent domainEvent)
            => _domainEvents.Add(domainEvent);

        public void ClearDomainEvents()
            => _domainEvents.Clear();
    }
}
