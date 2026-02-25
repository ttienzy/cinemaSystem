using Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Infrastructure.Persistence.Interceptors
{
    /// <summary>
    /// EF Core SaveChanges interceptor that automatically dispatches
    /// domain events after entities are saved to the database.
    /// This decouples side effects (email, SignalR, cache) from the domain logic.
    /// </summary>
    public class DomainEventDispatcherInterceptor(IMediator mediator) : SaveChangesInterceptor
    {
        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken ct = default)
        {
            if (eventData.Context is not null)
                await DispatchDomainEventsAsync(eventData.Context, ct);

            return result;
        }

        public override int SavedChanges(
            SaveChangesCompletedEventData eventData,
            int result)
        {
            if (eventData.Context is not null)
                DispatchDomainEventsAsync(eventData.Context).GetAwaiter().GetResult();

            return result;
        }

        private async Task DispatchDomainEventsAsync(DbContext context, CancellationToken ct = default)
        {
            var entities = context.ChangeTracker
                .Entries<BaseEntity>()
                .Where(e => e.Entity.DomainEvents.Any())
                .Select(e => e.Entity)
                .ToList();

            var events = entities
                .SelectMany(e => e.DomainEvents)
                .ToList();

            // Clear events BEFORE publishing to prevent infinite loops
            entities.ForEach(e => e.ClearDomainEvents());

            foreach (var domainEvent in events)
                await mediator.Publish(domainEvent, ct);
        }
    }
}
