using Application.Interfaces.Persistences.Repo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Shared.Common.Base;
using Shared.Common.Paging;
using Shared.Models.DataModels.InventoryDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repositories
{
    public class ConcessionSaleRepository : IConcessionSaleRepository
    {
        private readonly BookingContext _context;
        public ConcessionSaleRepository(BookingContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ConcessionRevenueResponse>> GetConcessionRevenueReportAsync(Guid cinemaId)
        {
            var queryable = _context.ConcessionSales
            .Where(c => c.CinemaId == cinemaId
                     && c.SaleDate.Date >= DateTime.Today.AddDays(-10))
            .GroupBy(c => c.SaleDate.Date)
            .Select(g => new ConcessionRevenueResponse
            {
                SaleDate = g.Key,
                TotalTransactions = g.Count(),
                TotalRevenue = g.Sum(s => s.TotalAmount)
            })
            .OrderBy(r => r.SaleDate);

            return await queryable.ToListAsync();
        }

        public async Task<PaginatedList<ConcessionSaleHistoryResponse>> GetConcessionSaleHistory(Guid cinemaId, ConcessionSaleQueryParameter query)
        {
            var queryable = _context.ConcessionSales.Where(c => c.CinemaId == cinemaId);

            if (query.FromDate.HasValue)
            {
                queryable = queryable.Where(x => x.SaleDate >= query.FromDate.Value);
            }

            if (query.ToDate.HasValue)
            {
                queryable = queryable.Where(x => x.SaleDate <= query.ToDate.Value);
            }

            if (!string.IsNullOrEmpty(query.PaymentMethod))
            {
                queryable = queryable.Where(x => x.PaymentMethod == query.PaymentMethod);
            }

            var totalCount = await queryable.CountAsync();

            var items = await queryable
                .OrderByDescending(x => x.SaleDate)
                .Skip((query.PageIndex - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(sale => new ConcessionSaleHistoryResponse
                {
                    SaleDate = sale.SaleDate,
                    TotalAmount = sale.TotalAmount,
                    PaymentMethod = sale.PaymentMethod!,
                    Items = _context.ConcessionSaleItems
                        .Where(item => item.ConcessionSaleId == sale.Id)
                        .Join(_context.InventoryItems,
                              item => item.InventoryId,
                              inv => inv.Id,
                              (item, inv) => new ConcessionSaleHistoryItem
                              {
                                  ItemName = inv.ItemName,
                                  Quantity = item.Quantity,
                                  UnitPrice = item.UnitPrice
                              })
                        .ToList(),
                    TicketSales = sale.BookingId != null
                        ? _context.Bookings
                            .Where(b => b.Id == sale.BookingId)
                            .Select(b => new TicketSaleHistoryResponse
                            {
                                TotalTickets = _context.BookingTickets.Count(bt => bt.BookingId == b.Id),
                                Status = b.Status
                            })
                            .FirstOrDefault()
                        : null
                })
                .ToListAsync();

            return new PaginatedList<ConcessionSaleHistoryResponse>(items, totalCount, query.PageIndex, query.PageSize);
        }
    }
}
