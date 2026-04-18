using Domain.Entities.BookingAggregate;
using Domain.Entities.ConcessionAggregate;
using Domain.Entities.InventoryAggregate;
using Domain.Entities.MovieAggregate;
using Domain.Entities.SharedAggregates;
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
        IRepository<SeatType> SeatTypes { get; }
        IRepository<InventoryItem> InventoryItems { get; }
        IRepository<ConcessionSale> ConcessionSales { get; }
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
