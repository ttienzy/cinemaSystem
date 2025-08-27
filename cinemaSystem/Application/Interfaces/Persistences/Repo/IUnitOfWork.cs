using Domain.Entities.BookingAggregate;
using Domain.Entities.MovieAggregate;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Persistences.Repo
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Booking> Bookings { get; }
        IRepository<Movie> Movies { get; }
        Task<int> SaveChangesAsync();
        Task BeginTractionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
