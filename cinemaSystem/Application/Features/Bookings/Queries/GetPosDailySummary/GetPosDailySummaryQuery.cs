using Application.Common.Interfaces.Persistence;
using Domain.Entities.BookingAggregate.Enums;
using Domain.Entities.StaffAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Models.DataModels.BookingDtos;

namespace Application.Features.Bookings.Queries.GetPosDailySummary
{
    public record GetPosDailySummaryQuery(Guid ShiftId, Guid StaffId) : IRequest<PosDailySummaryDto>;

    public class GetPosDailySummaryHandler(
        IBookingRepository bookingRepo,
        IConcessionSaleRepository concessionRepo,
        IShiftRepository shiftRepo,
        IStaffRepository staffRepo) : IRequestHandler<GetPosDailySummaryQuery, PosDailySummaryDto>
    {
        public async Task<PosDailySummaryDto> Handle(GetPosDailySummaryQuery request, CancellationToken ct)
        {
            // Get staff to find cinemaId
            var staff = await staffRepo.GetByIdWithCinemaAsync(request.StaffId, ct);
            if (staff == null)
            {
                return new PosDailySummaryDto
                {
                    ShiftId = request.ShiftId,
                    GeneratedAt = DateTime.UtcNow
                };
            }

            var cinemaId = staff.CinemaId;

            // Get shift info
            var shift = await shiftRepo.GetByIdAsync(request.ShiftId, ct);
            if (shift == null)
            {
                return new PosDailySummaryDto
                {
                    ShiftId = request.ShiftId,
                    GeneratedAt = DateTime.UtcNow
                };
            }

            var today = DateTime.UtcNow.Date;

            // Get bookings for this cinema and date
            var bookings = await bookingRepo.GetQueryable()
                .Where(b => b.CinemaId == cinemaId
                    && b.BookingTime.Date == today)
                .ToListAsync(ct);

            // Calculate ticket stats
            var validBookings = bookings
                .Where(b => b.Status != BookingStatus.Cancelled
                    && b.Status != BookingStatus.Refunded)
                .ToList();

            var totalTicketsSold = validBookings.Sum(b => b.TotalTickets);
            var ticketRevenue = validBookings.Sum(b => b.TotalAmount);

            // Get concession sales
            var concessionSales = await concessionRepo.GetQueryable()
                .Where(c => c.SaleDate.Date == today)
                .ToListAsync(ct);

            var totalConcessionItems = concessionSales.Sum(c => c.Items?.Sum(i => i.Quantity) ?? 0);
            var concessionRevenue = concessionSales.Sum(c => c.TotalAmount);

            // Booking stats
            var totalBookings = bookings.Count;
            var cancelledBookings = bookings.Count(b => b.Status == BookingStatus.Cancelled);
            var refundedBookings = bookings.Count(b => b.Status == BookingStatus.Refunded);

            return new PosDailySummaryDto
            {
                ShiftId = request.ShiftId,
                Date = today,
                ShiftName = shift.Name ?? "Unknown",
                TotalTicketsSold = totalTicketsSold,
                TicketRevenue = ticketRevenue,
                TotalConcessionItems = totalConcessionItems,
                ConcessionRevenue = concessionRevenue,
                TotalBookings = totalBookings,
                CancelledBookings = cancelledBookings,
                RefundedBookings = refundedBookings,
                TotalRevenue = ticketRevenue + concessionRevenue,
                GeneratedAt = DateTime.UtcNow
            };
        }
    }
}
