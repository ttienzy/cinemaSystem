namespace Application.Common.Interfaces.Persistence
{
    /// <summary>
    /// Simplified Unit of Work — manages transaction boundaries only.
    /// Does NOT contain repository properties (repositories are injected separately).
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        Task<int> SaveChangesAsync(CancellationToken ct = default);
        Task BeginTransactionAsync(CancellationToken ct = default);
        Task CommitTransactionAsync(CancellationToken ct = default);
        Task RollbackTransactionAsync(CancellationToken ct = default);
    }
}
