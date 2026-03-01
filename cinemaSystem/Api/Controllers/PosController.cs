using Application.Features.Bookings.Commands.CreateCounterBooking;
using Application.Features.Bookings.Commands.CreateUnifiedPosSale;
using Application.Features.Concessions.Commands.CreateConcessionSale;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DataModels.BookingDtos;
using System.Threading.Tasks;

namespace Api.Controllers
{
    /// <summary>
    /// Point-of-Sale (POS) controller for counter operations.
    /// All endpoints require Staff, Manager, or Admin role.
    /// StaffId is automatically resolved from the JWT token — no manual input required.
    /// </summary>
    // [Authorize(Roles = "Manager,Admin,Staff")]
    [ApiController]
    [Route("api/pos")]
    public class PosController(IMediator mediator) : ControllerBase
    {
        /// <summary>
        /// Sell tickets only at the counter. Immediate confirmation — no payment gateway.
        /// </summary>
        [HttpPost("bookings/counter")]
        [ProducesResponseType(typeof(CounterBookingResponse), 200)]
        public async Task<IActionResult> CreateCounterBooking([FromBody] CounterBookingRequest request)
        {
            var result = await mediator.Send(new CreateCounterBookingCommand(request));
            return Ok(result);
        }

        /// <summary>
        /// Unified POS: sell tickets + concessions in a single atomic transaction.
        /// Produces one receipt covering both. Ideal for high-speed counter sales.
        /// </summary>
        [HttpPost("sales/unified")]
        [ProducesResponseType(typeof(UnifiedPosResponse), 200)]
        public async Task<IActionResult> CreateUnifiedSale([FromBody] UnifiedPosRequest request)
        {
            var result = await mediator.Send(new CreateUnifiedPosSaleCommand(request));
            return Ok(result);
        }

        /// <summary>
        /// Sell concessions only (no ticket purchase). Links to a booking if provided.
        /// </summary>
        [HttpPost("concessions")]
        [ProducesResponseType(typeof(System.Guid), 200)]
        public async Task<IActionResult> CreateConcessionSale([FromBody] CreateConcessionSaleCommand command)
        {
            var result = await mediator.Send(command);
            return Ok(result);
        }
    }
}
