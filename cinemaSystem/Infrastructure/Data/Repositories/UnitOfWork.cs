using Application.Interfaces.Persistences.Repo;
using Domain.Entities.BookingAggregate;
using Domain.Entities.MovieAggregate;
using Domain.Entities.SharedAggregates;
using Domain.Entities.InventoryAggregate;
using Domain.Entities.ConcessionAggregate;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly BookingContext _context;
        public UnitOfWork(BookingContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IRepository<Booking> Bookings => new EfRepository<Booking>(_context);
        public IRepository<Movie> Movies => new EfRepository<Movie>(_context);

        public IRepository<SeatType> SeatTypes => new EfRepository<SeatType>(_context);
        public IRepository<InventoryItem> InventoryItems => new EfRepository<InventoryItem>(_context);
        public IRepository<ConcessionSale> ConcessionSales => new EfRepository<ConcessionSale>(_context);

        public async Task BeginTransactionAsync() => await _context.Database.BeginTransactionAsync();

        public async Task CommitTransactionAsync() => await _context.Database.CommitTransactionAsync();

        public void Dispose()
        {
            _context?.Dispose();
        }

        public async Task RollbackTransactionAsync()
        {
            await _context.Database.RollbackTransactionAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
