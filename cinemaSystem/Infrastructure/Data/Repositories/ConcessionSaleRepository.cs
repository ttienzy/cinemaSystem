using Application.Interfaces.Persistences.Repo;
using Microsoft.Data.SqlClient;
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

        public async Task<IEnumerable<RevenueMonthlyReportResponseDto>> GetMonthlyRevenueReportAsync(RevenueMonthlyReportRequestDto request)
        {
            var query = @"
                SELECT 
                    FORMAT(COALESCE(cs.SaleDate, b.BookingTime), 'yyyy-MM') AS [Month],
                    SUM(ISNULL(b.TotalAmount, 0)) AS TicketRevenue,
                    SUM(ISNULL(b.TotalTickets, 0)) AS TicketCount,
                    CASE 
                        WHEN SUM(ISNULL(b.TotalTickets, 0)) > 0 
                        THEN SUM(ISNULL(b.TotalAmount, 0)) / SUM(ISNULL(b.TotalTickets, 0))
                        ELSE 0 
                    END AS AverageTicketPrice,
                    SUM(ISNULL(csi.Quantity * csi.UnitPrice, 0)) AS ConcessionRevenue,
                    SUM(ISNULL(csi.Quantity, 0)) AS ConcessionCount,
                    CASE 
                        WHEN SUM(ISNULL(csi.Quantity, 0)) > 0 
                        THEN SUM(ISNULL(csi.Quantity * csi.UnitPrice, 0)) / SUM(ISNULL(csi.Quantity, 0))
                        ELSE 0 
                    END AS AverageConcessionPrice,
                    SUM(ISNULL(b.TotalAmount, 0) + ISNULL(csi.Quantity * csi.UnitPrice, 0)) AS TotalRevenue,
                    CASE 
                        WHEN SUM(ISNULL(b.TotalAmount, 0) + ISNULL(csi.Quantity * csi.UnitPrice, 0)) = 0 THEN 0
                        ELSE 
                            ROUND(
                                (SUM(ISNULL(csi.Quantity * csi.UnitPrice, 0)) * 100.0) / 
                                (SUM(ISNULL(b.TotalAmount, 0) + ISNULL(csi.Quantity * csi.UnitPrice, 0))), 2
                            )
                    END AS ConcessionRatioPercent
                FROM 
                    Bookings b
                    FULL OUTER JOIN ConcessionSales cs 
                        ON b.Id = cs.BookingId
                    LEFT JOIN ConcessionSaleItems csi 
                        ON cs.Id = csi.ConcessionSaleId
                WHERE 
                    cs.CinemaId = @CinemaId
                    AND CONVERT(date, COALESCE(cs.SaleDate, b.BookingTime)) BETWEEN @StartDate AND @EndDate
                GROUP BY 
                    FORMAT(COALESCE(cs.SaleDate, b.BookingTime), 'yyyy-MM')
                ORDER BY 
                    [Month];
            ";

            var parameters = new[]
            {
                new SqlParameter("@CinemaId", request.CinemaId),
                new SqlParameter("@StartDate", request.StartDate),
                new SqlParameter("@EndDate", request.EndDate)
            };

            var result = await _context.Database
                .SqlQueryRaw<RevenueMonthlyReportResponseDto>(query, parameters)
                .AsNoTracking()
                .ToListAsync();

            return result;
        }

        public async Task<IEnumerable<RevenueReportResponseDto>> GetRevenueReportAsync(RevenueReportRequestDto request)
        {
            var query = @"
                SELECT 
                    CONVERT(date, COALESCE(cs.SaleDate, b.BookingTime)) AS [Date],
                    SUM(ISNULL(b.TotalAmount, 0)) AS TicketRevenue,
                    SUM(ISNULL(b.TotalTickets, 0)) AS TicketCount,
                    CASE 
                        WHEN SUM(ISNULL(b.TotalTickets, 0)) > 0 
                        THEN SUM(ISNULL(b.TotalAmount, 0)) / SUM(ISNULL(b.TotalTickets, 0))
                        ELSE 0 
                    END AS AverageTicketPrice,
                    SUM(ISNULL(csi.Quantity * csi.UnitPrice, 0)) AS ConcessionRevenue,
                    SUM(ISNULL(csi.Quantity, 0)) AS ConcessionCount,
                    CASE 
                        WHEN SUM(ISNULL(csi.Quantity, 0)) > 0 
                        THEN SUM(ISNULL(csi.Quantity * csi.UnitPrice, 0)) / SUM(ISNULL(csi.Quantity, 0))
                        ELSE 0 
                    END AS AverageConcessionPrice,
                    SUM(ISNULL(b.TotalAmount, 0) + ISNULL(csi.Quantity * csi.UnitPrice, 0)) AS TotalRevenue,
                    CASE 
                        WHEN SUM(ISNULL(b.TotalAmount, 0) + ISNULL(csi.Quantity * csi.UnitPrice, 0)) = 0 THEN 0
                        ELSE 
                            ROUND(
                                (SUM(ISNULL(csi.Quantity * csi.UnitPrice, 0)) * 100.0) / 
                                (SUM(ISNULL(b.TotalAmount, 0) + ISNULL(csi.Quantity * csi.UnitPrice, 0))), 2
                            )
                    END AS ConcessionRatioPercent
                FROM 
                    Bookings b
                    FULL OUTER JOIN ConcessionSales cs 
                        ON b.Id = cs.BookingId
                    LEFT JOIN ConcessionSaleItems csi 
                        ON cs.Id = csi.ConcessionSaleId
                WHERE 
                    cs.CinemaId = @CinemaId
                    AND CONVERT(date, COALESCE(cs.SaleDate, b.BookingTime)) BETWEEN @StartDate AND @EndDate
                GROUP BY 
                    CONVERT(date, COALESCE(cs.SaleDate, b.BookingTime))
                ORDER BY 
                    [Date];
            ";

            var parameters = new[]
    {
        new SqlParameter("@CinemaId", request.CinemaId),
        new SqlParameter("@StartDate", request.StartDate),
        new SqlParameter("@EndDate", request.EndDate)
    };

            var result = await _context.Database
                .SqlQueryRaw<RevenueReportResponseDto>(query, parameters)
                .AsNoTracking()
                .ToListAsync();

            return result;
        }
    }
}
