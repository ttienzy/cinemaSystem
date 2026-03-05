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
    /// Quản lý booking tại rạp — Dành cho Manager.
    /// Manager xem booking tại rạp mình, theo dõi hôm nay, duyệt hoàn tiền.
    /// </summary>
    [ApiController]
    [Route("api/manager/bookings")]
    [Authorize(Roles = "Manager,Admin")]
    public class ManagerBookingsController(
        IMediator mediator,
        IBookingRepository bookingRepo) : ControllerBase
    {
        /// <summary>
        /// Danh sách booking tại rạp — lọc theo trạng thái, ngày, phân trang.
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

            // Lọc theo trạng thái booking
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<BookingStatus>(status, true, out var bookingStatus))
                query = query.Where(b => b.Status == bookingStatus);

            // Lọc theo ngày
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
        /// Booking hôm nay tại rạp — overview nhanh cho Manager.
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
        /// Danh sách yêu cầu hoàn tiền chờ duyệt tại rạp.
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
        /// Duyệt yêu cầu hoàn tiền — Manager xác nhận hoàn tiền cho khách.
        /// </summary>
        [HttpPost("{id:guid}/approve-refund")]
        public async Task<IActionResult> ApproveRefund(Guid id)
        {
            await mediator.Send(new ApproveRefundCommand(id));
            return Ok(new { message = "Đã duyệt hoàn tiền thành công." });
        }
    }
}
