using Application.Features.Cinemas.Commands.CreateCinema;
using Application.Features.Cinemas.Commands.UpdateCinema;
using Application.Features.Cinemas.Commands.DeleteCinema;
using Application.Features.Cinemas.Commands.CreateScreen;
using Application.Features.Cinemas.Commands.CreateSeatsBulk;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DataModels.CinemaDtos;

namespace Api.Controllers
{
    /// <summary>
    /// Handles admin cinema management APIs.
    /// </summary>
    public class AdminCinemasController : BaseApiController
    {
        /// <summary>
        /// Create a new cinema.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Guid>> CreateCinema([FromBody] CinemaUpsertRequest request)
        {
            return Ok(await Mediator.Send(new CreateCinemaCommand(request)));
        }

        /// <summary>
        /// Update an existing cinema.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCinema(Guid id, [FromBody] CinemaUpsertRequest request)
        {
            await Mediator.Send(new UpdateCinemaCommand(id, request));
            return NoContent();
        }

        /// <summary>
        /// Delete a cinema.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCinema(Guid id)
        {
            await Mediator.Send(new DeleteCinemaCommand(id));
            return NoContent();
        }

        /// <summary>
        /// Create a new screen in a cinema.
        /// </summary>
        [HttpPost("{cinemaId}/screens")]
        public async Task<ActionResult<Guid>> CreateScreen(Guid cinemaId, [FromBody] ScreenRequest request)
        {
            return Ok(await Mediator.Send(new CreateScreenCommand(cinemaId, request)));
        }

        /// <summary>
        /// Bulk create seats for a screen.
        /// </summary>
        [HttpPost("screens/{screenId}/seats/bulk")]
        public async Task<ActionResult<List<Guid>>> CreateSeatsBulk(Guid screenId, [FromBody] List<SeatGenerateRequest> requests)
        {
            return Ok(await Mediator.Send(new CreateSeatsBulkCommand(screenId, requests)));
        }
    }
}
