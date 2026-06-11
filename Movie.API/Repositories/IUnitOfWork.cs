namespace Movie.API.Repositories;

public interface IUnitOfWork
{
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
    Task<int> SaveChangesAsync();
    Task ExecuteInTransactionAsync(Func<Task> action);
}
