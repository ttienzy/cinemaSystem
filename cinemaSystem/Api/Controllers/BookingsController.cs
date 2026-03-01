using Application.Features.Bookings.Commands.ApproveRefund;
using Application.Features.Bookings.Commands.RequestRefund;
using Application.Features.Bookings.Commands.CancelBooking;
using Application.Features.Bookings.Commands.CompleteBooking;
using Application.Features.Bookings.Commands.CreateBooking;
using Application.Features.Bookings.Queries.GetBookingById;
using Application.Features.Bookings.Queries.GetMyBookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    //[Authorize]
    public class BookingsController : BaseApiController
    {
        [HttpPost]
        public async Task<ActionResult<CreateBookingResult>> CreateBooking([FromBody] CreateBookingCommand command)
        {
            // Ensure the customer ID from command matches the user ID from token (or just use token)
            // For now, let's trust the command or override it for safety:
            // var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return Ok(await Mediator.Send(command));
        }

        [HttpPost("{id}/complete")]
        public async Task<IActionResult> CompleteBooking(Guid id, [FromBody] CompleteBookingRequest request)
        {
            await Mediator.Send(new CompleteBookingCommand(id, request.TransactionId, request.ReferenceCode));
            return NoContent();
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelBooking(Guid id, [FromBody] CancelBookingRequest request)
        {
            await Mediator.Send(new CancelBookingCommand(id, request.Reason));
            return NoContent();
        }

        [HttpPost("{id}/request-refund")]
        public async Task<IActionResult> RequestRefund(Guid id, [FromBody] RefundRequest request)
        {
            await Mediator.Send(new RequestRefundCommand(id, request.Reason));
            return NoContent();
        }

        [HttpPost("{id}/approve-refund")]
        //[Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> ApproveRefund(Guid id)
        {
            await Mediator.Send(new ApproveRefundCommand(id));
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookingDetailDto>> GetBooking(Guid id)
        {
            return Ok(await Mediator.Send(new GetBookingByIdQuery(id)));
        }

        [HttpGet("my")]
        public async Task<ActionResult<MyBookingsResult>> GetMyBookings([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return Ok(await Mediator.Send(new GetMyBookingsQuery(userId, page, pageSize)));
        }
    }

    public record CompleteBookingRequest(string TransactionId, string ReferenceCode);
    public record CancelBookingRequest(string Reason);
    public record RefundRequest(string Reason);
}
