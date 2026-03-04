using Application.Features.Bookings.Commands.CreateCounterBooking;
using Application.Features.Bookings.Commands.CreateUnifiedPosSale;
using Application.Features.Concessions.Commands.CreateSale;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DataModels.BookingDtos;

namespace Api.Controllers
{
    /// <summary>
    /// Point-of-Sale (POS) APIs for counter operations.
    /// </summary>
    [ApiController]
    [Route("api/pos")]
    public class PosController(IMediator mediator) : ControllerBase
    {
        /// <summary>
        /// Sell tickets at counter.
        /// </summary>
        [HttpPost("bookings/counter")]
        public async Task<IActionResult> CreateCounterBooking([FromBody] CounterBookingRequest request)
        {
            var result = await mediator.Send(new CreateCounterBookingCommand(request));
            return Ok(result);
        }

        /// <summary>
        /// Unified POS: sell tickets and concessions in one transaction.
        /// </summary>
        [HttpPost("sales/unified")]
        public async Task<IActionResult> CreateUnifiedSale([FromBody] UnifiedPosRequest request)
        {
            var result = await mediator.Send(new CreateUnifiedPosSaleCommand(request));
            return Ok(result);
        }

        /// <summary>
        /// Sell concessions only.
        /// </summary>
        [HttpPost("concessions")]
        public async Task<IActionResult> CreateConcessionSale([FromBody] CreateConcessionSaleCommand command)
        {
            var result = await mediator.Send(command);
            return Ok(result);
        }
    }
}
