using Application.Common.Interfaces.Security;
using Application.Features.Bookings.Commands.CreateCounterBooking;
using Application.Features.Bookings.Commands.CreateUnifiedPosSale;
using Application.Features.Bookings.Queries.GetPosDailySummary;
using Application.Features.Concessions.Commands.CreateSale;
using Application.Features.Showtimes.Queries.GetShowtimesByCinema;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DataModels.BookingDtos;
using Shared.Models.DataModels.ShowtimeDtos;
using System.Security.Claims;

namespace Api.Controllers
{
    /// <summary>
    /// Point-of-Sale (POS) — Dành cho Staff và Manager tại quầy.
    /// Bao gồm: Bán vé tại quầy, bán bắp nước, bán combo, xem lịch chiếu hôm nay.
    /// Tự động lấy StaffId từ token.
    /// </summary>
    [ApiController]
    [Route("api/pos")]
    [Authorize(Roles = "Staff,Manager")]
    public class PosEnhancedController(IMediator mediator) : ControllerBase
    {
        /// <summary>
        /// Lịch chiếu hôm nay tại rạp — hiển thị trên màn hình POS.
        /// Staff chọn suất chiếu để bán vé.
        /// </summary>
        [HttpGet("showtimes/today")]
        public async Task<ActionResult<List<ShowtimeResponse>>> GetTodayShowtimes([FromQuery] Guid cinemaId)
        {
            var result = await mediator.Send(new GetShowtimesByCinemaQuery(cinemaId, DateTime.Today));
            return Ok(result);
        }

        /// <summary>
        /// In vé — trả về thông tin đầy đủ để in (phim, suất, ghế, giá, mã QR).
        /// </summary>
        [HttpGet("bookings/{id:guid}/print")]
        public async Task<IActionResult> GetPrintData(Guid id)
        {
            var booking = await mediator.Send(new Application.Features.Bookings.Queries.GetBookingById.GetBookingByIdQuery(id));
            return Ok(new
            {
                booking,
                PrintedAt = DateTime.UtcNow,
                PrintType = "TICKET"
            });
        }

        /// <summary>
        /// Tổng kết doanh thu trong ca — vé bán, bắp nước, tổng doanh thu.
        /// </summary>
        [HttpGet("sales/daily-summary")]
        public async Task<ActionResult<PosDailySummaryDto>> GetDailySummary([FromQuery] Guid shiftId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return Ok(await mediator.Send(new GetPosDailySummaryQuery(shiftId, userId)));
        }
    }
}
