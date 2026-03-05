using Application.Common.Interfaces.Services;
using Application.Features.Bookings.Commands.ApproveRefund;
using Application.Features.Bookings.Commands.RequestRefund;
using Application.Features.Bookings.Commands.CancelBooking;
using Application.Features.Bookings.Commands.CompleteBooking;
using Application.Features.Bookings.Commands.CreateBooking;
using Application.Features.Bookings.Commands.CheckIn;
using Application.Features.Bookings.Queries.GetBookingById;
using Application.Features.Bookings.Queries.GetMyBookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    /// <summary>
    /// Handles booking-related APIs.
    /// </summary>
    public class BookingsController : BaseApiController
    {
        /// <summary>
        /// Create a new booking.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CreateBookingResult>> CreateBooking([FromBody] CreateBookingCommand command)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var securedCommand = command with { CustomerId = userId, ClientIpAddress = ip };
            return Ok(await Mediator.Send(securedCommand));
        }

        [AllowAnonymous]
        /// <summary>
        /// Payment callback endpoint from payment gateway.
        /// </summary>
        [HttpGet("callback")]
        public async Task<IActionResult> PaymentCallback()
        {
            var callbackParams = HttpContext.Request.Query
                .ToDictionary(q => q.Key, q => q.Value.ToString());

            var paymentGateway = HttpContext.RequestServices.GetRequiredService<IPaymentGateway>();
            var result = paymentGateway.ProcessCallback(callbackParams);

            if (result.IsSuccess)
            {
                await Mediator.Send(new CompleteBookingCommand(
                    result.BookingId,
                    result.TransactionId ?? string.Empty,
                    result.ReferenceCode ?? string.Empty));

                return Redirect($"http://localhost:5173/booking/success?bookingId={result.BookingId}");
            }

            return Redirect($"http://localhost:5173/booking/failed?error={Uri.EscapeDataString(result.ErrorMessage ?? "Payment failed")}");
        }

        /// <summary>
        /// Mark booking as completed.
        /// </summary>
        [HttpPost("{id}/complete")]
        public async Task<IActionResult> CompleteBooking(Guid id, [FromBody] CompleteBookingRequest request)
        {
            await Mediator.Send(new CompleteBookingCommand(id, request.TransactionId, request.ReferenceCode));
            return NoContent();
        }

        /// <summary>
        /// Cancel a booking.
        /// </summary>
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelBooking(Guid id, [FromBody] CancelBookingRequest request)
        {
            await Mediator.Send(new CancelBookingCommand(id, request.Reason));
            return NoContent();
        }

        /// <summary>
        /// Request refund for a booking.
        /// </summary>
        [HttpPost("{id}/request-refund")]
        public async Task<IActionResult> RequestRefund(Guid id, [FromBody] RefundRequest request)
        {
            await Mediator.Send(new RequestRefundCommand(id, request.Reason));
            return NoContent();
        }

        /// <summary>
        /// Approve refund request for a booking.
        /// </summary>
        [HttpPost("{id}/approve-refund")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveRefund(Guid id)
        {
            await Mediator.Send(new ApproveRefundCommand(id));
            return NoContent();
        }

        /// <summary>
        /// Get booking details by ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<BookingDetailDto>> GetBooking(Guid id)
        {
            return Ok(await Mediator.Send(new GetBookingByIdQuery(id)));
        }

        /// <summary>
        /// Get current user's bookings.
        /// </summary>
        [HttpGet("my")]
        public async Task<ActionResult<MyBookingsResult>> GetMyBookings([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return Ok(await Mediator.Send(new GetMyBookingsQuery(userId, page, pageSize)));
        }

        /// <summary>
        /// Check-in by booking ID.
        /// </summary>
        [HttpPost("{id}/check-in")]
        [Authorize(Roles = "Staff,Manager")]
        public async Task<ActionResult<CheckInResult>> CheckIn(Guid id)
        {
            var result = await Mediator.Send(new CheckInBookingCommand(id));
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Check-in by QR code.
        /// </summary>
        [HttpPost("check-in")]
        [Authorize(Roles = "Staff,Manager")]
        public async Task<ActionResult<CheckInResult>> CheckInByQrCode([FromBody] CheckInByQrCodeRequest request)
        {
            var result = await Mediator.Send(new CheckInBookingCommand(request.BookingId, request.CheckInToken));
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }

    public record CompleteBookingRequest(string TransactionId, string ReferenceCode);
    public record CancelBookingRequest(string Reason);
    public record RefundRequest(string Reason);
    public record CheckInByQrCodeRequest(Guid BookingId, string CheckInToken);
}
