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
    /// Point-of-Sale (POS) — For Counter Staff and Managers.
    /// Includes: Counter ticket sales, concession sales, combo sales, and viewing today's showtimes.
    /// Automatically retrieves StaffId from token.
    /// </summary>
    [ApiController]
    [Route("api/pos")]
    // [Authorize(Roles = "Staff,Manager")]
    public class PosEnhancedController(IMediator mediator) : ControllerBase
    {
        /// <summary>
        /// Today's showtimes at the cinema — displayed on the POS screen.
        /// Staff select showtimes to sell tickets.
        /// </summary>
        [HttpGet("showtimes/today")]
        public async Task<ActionResult<List<ShowtimeResponse>>> GetTodayShowtimes([FromQuery] Guid cinemaId)
        {
            var result = await mediator.Send(new GetShowtimesByCinemaQuery(cinemaId, DateTime.Today));
            return Ok(result);
        }

        /// <summary>
        /// Print ticket — returns full information for printing (movie, showtime, seat, price, QR code).
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
        /// Shift revenue summary — tickets sold, concessions, total revenue.
        /// </summary>
        [HttpGet("sales/daily-summary")]
        public async Task<ActionResult<PosDailySummaryDto>> GetDailySummary([FromQuery] Guid shiftId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return Ok(await mediator.Send(new GetPosDailySummaryQuery(shiftId, userId)));
        }
    }
}
