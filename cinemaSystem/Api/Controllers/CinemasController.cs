using Application.Features.Cinemas.Commands.BlockSeat;
using Application.Features.Cinemas.Commands.LinkSeat;
using Application.Features.Cinemas.Commands.UnlinkSeat;
using Application.Features.Cinemas.Queries.GetAllCinemas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DataModels.CinemaDtos;

namespace Api.Controllers
{
    /// <summary>
    /// Handles cinema and seat management APIs.
    /// </summary>
    public class CinemasController : BaseApiController
    {
        /// <summary>
        /// Get all cinemas.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<CinemaSummaryResponse>>> GetCinemas()
        {
            return Ok(await Mediator.Send(new GetAllCinemasQuery()));
        }

        /// <summary>
        /// Block a seat in a screen.
        /// </summary>
        [HttpPost("screens/{screenId}/seats/{seatId}/block")]
        public async Task<IActionResult> BlockSeat(Guid screenId, Guid seatId, [FromBody] BlockSeatRequest request)
        {
            await Mediator.Send(new BlockSeatCommand(screenId, seatId, request.Reason));
            return NoContent();
        }

        /// <summary>
        /// Link a seat to another seat (couple seats).
        /// </summary>
        [HttpPost("screens/{screenId}/seats/{seatId}/link")]
        public async Task<IActionResult> LinkSeat(Guid screenId, Guid seatId, [FromBody] LinkSeatRequest request)
        {
            await Mediator.Send(new LinkSeatCommand(screenId, seatId, request.PartnerSeatNumber));
            return NoContent();
        }

        /// <summary>
        /// Unlink a seat from its partner.
        /// </summary>
        [HttpPost("screens/{screenId}/seats/{seatId}/unlink")]
        public async Task<IActionResult> UnlinkSeat(Guid screenId, Guid seatId)
        {
            await Mediator.Send(new UnlinkSeatCommand(screenId, seatId));
            return NoContent();
        }
    }

    public record BlockSeatRequest(string Reason);
    public record LinkSeatRequest(int PartnerSeatNumber);
}
