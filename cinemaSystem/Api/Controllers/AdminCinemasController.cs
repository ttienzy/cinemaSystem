using Application.Features.Cinemas.Commands.CreateCinema;
using Application.Features.Cinemas.Commands.UpdateCinema;
using Application.Features.Cinemas.Commands.DeleteCinema;
using Application.Features.Cinemas.Commands.CreateScreen;
using Application.Features.Cinemas.Commands.CreateSeatsBulk;
using Application.Features.Cinemas.Commands.UpdateScreen;
using Application.Features.Cinemas.Commands.DeleteScreen;
using Application.Features.Cinemas.Commands.UpdateSeat;
using Application.Features.Cinemas.Commands.DeleteSeat;
using Application.Features.Cinemas.Queries.GetScreens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Features.Cinemas.Queries.GetScreens;
using Shared.Models.DataModels.CinemaDtos;

namespace Api.Controllers
{
    /// <summary>
    /// Cinema management — Admin only.
    /// Includes: Cinema CRUD, Screen creation, and Bulk Seat creation.
    /// </summary>
    [Route("api/admin/cinemas")]
    // [Authorize(Roles = "Admin")]
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
        /// Get all screens in a cinema.
        /// </summary>
        [HttpGet("{cinemaId}/screens")]
        public async Task<ActionResult<List<ScreenResponse>>> GetScreens(Guid cinemaId)
        {
            return Ok(await Mediator.Send(new GetScreensQuery(cinemaId)));
        }

        /// <summary>
        /// Bulk create seats for a screen.
        /// </summary>
        [HttpPost("screens/{screenId}/seats/bulk")]
        public async Task<ActionResult<List<Guid>>> CreateSeatsBulk(Guid screenId, [FromBody] List<SeatGenerateRequest> requests)
        {
            return Ok(await Mediator.Send(new CreateSeatsBulkCommand(screenId, requests)));
        }

        /// <summary>
        /// Update a screen in a cinema.
        /// </summary>
        [HttpPut("{cinemaId}/screens/{screenId}")]
        public async Task<IActionResult> UpdateScreen(Guid cinemaId, Guid screenId, [FromBody] ScreenUpdateRequest request)
        {
            await Mediator.Send(new UpdateScreenCommand(cinemaId, screenId, request.ScreenName, request.ScreenType, request.Status));
            return NoContent();
        }

        /// <summary>
        /// Delete (soft) a screen in a cinema.
        /// </summary>
        [HttpDelete("{cinemaId}/screens/{screenId}")]
        public async Task<IActionResult> DeleteScreen(Guid cinemaId, Guid screenId)
        {
            await Mediator.Send(new DeleteScreenCommand(cinemaId, screenId));
            return NoContent();
        }

        /// <summary>
        /// Update a seat in a screen.
        /// </summary>
        [HttpPut("screens/{screenId}/seats/{seatId}")]
        public async Task<IActionResult> UpdateSeat(Guid cinemaId, Guid screenId, Guid seatId, [FromBody] SeatUpdateRequest request)
        {
            await Mediator.Send(new UpdateSeatCommand(cinemaId, screenId, seatId, request.SeatTypeId, request.RowName, request.Number, request.IsActive));
            return NoContent();
        }

        /// <summary>
        /// Delete (soft) a seat in a screen.
        /// </summary>
        [HttpDelete("screens/{screenId}/seats/{seatId}")]
        public async Task<IActionResult> DeleteSeat(Guid cinemaId, Guid screenId, Guid seatId)
        {
            await Mediator.Send(new DeleteSeatCommand(cinemaId, screenId, seatId));
            return NoContent();
        }
    }

    public record ScreenUpdateRequest(string ScreenName, string ScreenType, string Status);
    public record SeatUpdateRequest(Guid SeatTypeId, string RowName, int Number, bool IsActive);
}
