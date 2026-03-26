using Application.Common.Interfaces.Persistence;
using Domain.Entities.BookingAggregate.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Models.DataModels.DashboardDtos;

namespace Application.Features.Dashboard.Queries.GetRevenueReport
{
    /// <summary>
    /// Revenue report handler — aggregates ticket + concession revenue over a time period.
    /// Supports grouping by day/week/month for dashboard charts.
    /// </summary>
    public class GetRevenueReportHandler(
        IBookingRepository bookingRepo,
        IConcessionSaleRepository concessionRepo)
        : IRequestHandler<GetRevenueReportQuery, RevenueReportDto>
    {
        public async Task<RevenueReportDto> Handle(GetRevenueReportQuery request, CancellationToken ct)
        {
            var from = request.From.Date;
            var to = request.To.Date.AddDays(1); // Bao gồm cả ngày cuối

            // === Get ticket revenue (completed bookings only) ===
            var bookingsQuery = bookingRepo.GetQueryable()
                .Where(b => b.Status == BookingStatus.Completed
                    && b.BookingTime >= from && b.BookingTime < to);

            // Filter by cinema if applicable (for Managers)
            if (request.CinemaId.HasValue)
            {
                bookingsQuery = bookingsQuery.Where(b => b.CinemaId == request.CinemaId.Value);
            }

            var bookings = await bookingsQuery
                .Select(b => new { b.BookingTime, b.TotalAmount })
                .ToListAsync(ct);

            // === Get concession revenue ===
            var concessionQuery = concessionRepo.GetQueryable()
                .Where(c => c.SaleDate >= from && c.SaleDate < to);

            if (request.CinemaId.HasValue)
            {
                concessionQuery = concessionQuery.Where(c => c.CinemaId == request.CinemaId.Value);
            }

            var concessions = await concessionQuery
                .Select(c => new { c.SaleDate, c.TotalAmount })
                .ToListAsync(ct);

            // === Group by day/week/month ===
            var items = new List<RevenueItemDto>();
            var current = from;

            while (current < to)
            {
                DateTime periodEnd;
                string label;

                switch (request.GroupBy.ToLower())
                {
                    case "week":
                        periodEnd = current.AddDays(7);
                        label = $"Week {current:dd/MM} - {periodEnd.AddDays(-1):dd/MM}";
                        break;
                    case "month":
                        periodEnd = current.AddMonths(1);
                        label = current.ToString("MM/yyyy");
                        break;
                    default: // day
                        periodEnd = current.AddDays(1);
                        label = current.ToString("dd/MM/yyyy");
                        break;
                }

                var ticketRev = bookings
                    .Where(b => b.BookingTime >= current && b.BookingTime < periodEnd)
                    .Sum(b => b.TotalAmount);

                var concRev = concessions
                    .Where(c => c.SaleDate >= current && c.SaleDate < periodEnd)
                    .Sum(c => c.TotalAmount);

                var bookingCount = bookings
                    .Count(b => b.BookingTime >= current && b.BookingTime < periodEnd);

                items.Add(new RevenueItemDto
                {
                    Label = label,
                    TicketRevenue = ticketRev,
                    ConcessionRevenue = concRev,
                    Total = ticketRev + concRev,
                    BookingCount = bookingCount
                });

                current = periodEnd;
            }

            return new RevenueReportDto
            {
                TotalTicketRevenue = bookings.Sum(b => b.TotalAmount),
                TotalConcessionRevenue = concessions.Sum(c => c.TotalAmount),
                GrandTotal = bookings.Sum(b => b.TotalAmount) + concessions.Sum(c => c.TotalAmount),
                From = request.From,
                To = request.To,
                Items = items
            };
        }
    }
}
