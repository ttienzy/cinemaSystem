using Application.Common.Interfaces.Persistence;
using Application.Features.Bookings.Commands.ApproveRefund;
using Domain.Entities.BookingAggregate.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    /// <summary>
    /// Cinema booking management — For Managers.
    /// Managers view bookings in their cinema, track today's activity, and approve refunds.
    /// </summary>
    [ApiController]
    [Route("api/manager/bookings")]
    // [Authorize(Roles = "Manager,Admin")]
    public class ManagerBookingsController(
        IMediator mediator,
        IBookingRepository bookingRepo) : ControllerBase
    {
        /// <summary>
        /// List of cinema bookings — filter by status, date, and pagination.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCinemaBookings(
            [FromQuery] Guid cinemaId,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? date = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = bookingRepo.GetQueryable()
                .Where(b => b.CinemaId == cinemaId);

            // Filter by booking status
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<BookingStatus>(status, true, out var bookingStatus))
                query = query.Where(b => b.Status == bookingStatus);

            // Filter by date
            if (date.HasValue)
                query = query.Where(b => b.BookingTime.Date == date.Value.Date);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(b => b.BookingTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new
                {
                    b.Id,
                    b.BookingCode,
                    b.BookingTime,
                    b.Status,
                    b.TotalAmount,
                    b.CustomerId,
                    b.ShowtimeId,
                    TicketCount = b.TotalTickets
                })
                .ToListAsync();

            return Ok(new { items, total, page, pageSize });
        }

        /// <summary>
        /// Today's cinema bookings — quick overview for Managers.
        /// </summary>
        [HttpGet("today")]
        public async Task<IActionResult> GetTodayBookings([FromQuery] Guid cinemaId)
        {
            var today = DateTime.UtcNow.Date;
            var bookings = await bookingRepo.GetQueryable()
                .Where(b => b.CinemaId == cinemaId && b.BookingTime.Date == today)
                .OrderByDescending(b => b.BookingTime)
                .Select(b => new
                {
                    b.Id,
                    b.BookingCode,
                    b.BookingTime,
                    b.Status,
                    b.TotalAmount,
                    TicketCount = b.TotalTickets
                })
                .ToListAsync();

            return Ok(bookings);
        }

        /// <summary>
        /// List of pending refund requests in the cinema.
        /// </summary>
        [HttpGet("refund-requests")]
        public async Task<IActionResult> GetRefundRequests([FromQuery] Guid cinemaId)
        {
            var refundRequests = await bookingRepo.GetQueryable()
                .Where(b => b.CinemaId == cinemaId
                    && b.Status == BookingStatus.PendingRefund)
                .OrderBy(b => b.BookingTime)
                .Select(b => new
                {
                    b.Id,
                    b.BookingCode,
                    b.BookingTime,
                    b.TotalAmount,
                    b.CustomerId,
                    TicketCount = b.BookingTickets.Count
                })
                .ToListAsync();

            return Ok(refundRequests);
        }

        /// <summary>
        /// Approve refund request — Manager confirms refund for customer.
        /// </summary>
        [HttpPost("{id:guid}/approve-refund")]
        public async Task<IActionResult> ApproveRefund(Guid id)
        {
            await mediator.Send(new ApproveRefundCommand(id));
            return Ok(new { message = "Refund approved successfully." });
        }
    }
}
