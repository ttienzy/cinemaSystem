using Ardalis.Specification;
using Domain.Common;

namespace Application.Interfaces.Persistences.Repo
{
    public interface IRepository<T> : IRepositoryBase<T> where T : class, IAggregateRoot
    {
    }
}
