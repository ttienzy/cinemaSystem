using Application.Features.Cinemas.Commands.BlockSeat;
using Application.Features.Cinemas.Commands.LinkSeat;
using Application.Features.Cinemas.Queries.GetAllCinemas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public class CinemasController : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<List<CinemaDto>>> GetCinemas()
        {
            return Ok(await Mediator.Send(new GetAllCinemasQuery()));
        }

        [HttpPost("screens/{screenId}/seats/{seatId}/block")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> BlockSeat(Guid screenId, Guid seatId, [FromBody] BlockSeatRequest request)
        {
            await Mediator.Send(new BlockSeatCommand(screenId, seatId, request.Reason));
            return NoContent();
        }

        [HttpPost("screens/{screenId}/seats/{seatId}/link")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> LinkSeat(Guid screenId, Guid seatId, [FromBody] LinkSeatRequest request)
        {
            await Mediator.Send(new LinkSeatCommand(screenId, seatId, request.PartnerSeatNumber));
            return NoContent();
        }
    }

    public record BlockSeatRequest(string Reason);
    public record LinkSeatRequest(int PartnerSeatNumber);
}
