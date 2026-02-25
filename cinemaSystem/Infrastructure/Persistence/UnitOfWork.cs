using Application.Common.Interfaces.Persistence;
using Infrastructure.Data;

namespace Infrastructure.Persistence
{
    /// <summary>
    /// Simplified UnitOfWork — manages transaction boundaries only.
    /// Repositories are injected independently, not contained here.
    /// </summary>
    public class UnitOfWork(BookingContext context) : IUnitOfWork
    {
        private bool _disposed;

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
            => await context.SaveChangesAsync(ct);

        public async Task BeginTransactionAsync(CancellationToken ct = default)
            => await context.Database.BeginTransactionAsync(ct);

        public async Task CommitTransactionAsync(CancellationToken ct = default)
            => await context.Database.CommitTransactionAsync(ct);

        public async Task RollbackTransactionAsync(CancellationToken ct = default)
            => await context.Database.RollbackTransactionAsync(ct);

        public void Dispose()
        {
            if (!_disposed)
            {
                context.Dispose();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}
