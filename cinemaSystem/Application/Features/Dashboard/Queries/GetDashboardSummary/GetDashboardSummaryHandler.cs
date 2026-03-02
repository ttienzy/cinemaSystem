using Application.Common.Interfaces.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Models.DataModels.DashboardDtos;
using Shared.Models.DataModels.DashboardDtos.Subs;

namespace Application.Features.Dashboard.Queries.GetDashboardSummary
{
    public class GetDashboardSummaryHandler(
        IBookingRepository bookingRepo,
        IShowtimeRepository showtimeRepo,
        IConcessionSaleRepository concessionRepo,
        IInventoryRepository inventoryRepo,
        ILogger<GetDashboardSummaryHandler> logger) : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
    {
        public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken ct)
        {
            var today = DateTime.UtcNow.Date;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            // Ticket revenue (bookings with Completed status)
            var ticketRevenue = await bookingRepo.GetQueryable()
                .Where(b => b.Status == Domain.Entities.BookingAggregate.Enums.BookingStatus.Completed
                    && b.BookingTime >= startOfMonth)
                .SumAsync(b => b.TotalAmount, ct);

            // Concession revenue
            var concessionRevenue = await concessionRepo.GetQueryable()
                .Where(c => c.SaleDate >= startOfMonth)
                .SumAsync(c => c.TotalAmount, ct);

            // Total bookings this month
            var totalBookings = await bookingRepo.GetQueryable()
                .CountAsync(b => b.BookingTime >= startOfMonth, ct);

            // Showtime occupancy (average)
            var showtimes = await showtimeRepo.GetQueryable()
                .Where(s => s.ShowDate >= today)
                .ToListAsync(ct);

            var occupancyRate = showtimes.Any()
                ? showtimes.Average(s => (double)s.BookedSeats / s.TotalSeats * 100)
                : 0;

            return new DashboardSummaryDto
            {
                TotalRevenue = ticketRevenue + concessionRevenue,
                TicketRevenue = ticketRevenue,
                ConcessionRevenue = concessionRevenue,
                TotalBookings = totalBookings,
                OccupancyRate = Math.Round(occupancyRate, 2),
                PeriodStart = startOfMonth,
                PeriodEnd = today
            };
        }
    }
}
