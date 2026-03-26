using Application.Features.Cinemas.Commands.CreateScreen;
using Application.Features.Cinemas.Commands.CreateSeatsBulk;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DataModels.CinemaDtos;

namespace Api.Controllers
{
    /// <summary>
    /// Genre management — Admin only.
    /// Admin creates/updates/deletes movie genres. Public GET endpoint is in GenresController.
    /// </summary>
    [ApiController]
    [Route("api/admin/genres")]
    // [Authorize(Roles = "Admin")]
    public class AdminGenresController : BaseApiController
    {
        /// <summary>
        /// Create a new genre — e.g., "Action", "Horror", "Romance".
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] Shared.Models.DataModels.SharedDtos.CreateGenreRequest request)
        {
            return Ok(await Mediator.Send(new Application.Features.Shared.Genres.Commands.Create.CreateGenreCommand(request)));
        }

        /// <summary>
        /// Update genre.
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Shared.Models.DataModels.SharedDtos.UpdateGenreRequest request)
        {
            await Mediator.Send(new Application.Features.Shared.Genres.Commands.Update.UpdateGenreCommand(id, request));
            return NoContent();
        }

        /// <summary>
        /// Delete genre — soft delete (deactivate).
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await Mediator.Send(new DeleteGenreCommand(id));
            return NoContent();
        }
    }

    public record DeleteGenreCommand(Guid Id) : IRequest;

    /// <summary>
    /// Seat Type management — Admin only.
    /// Admin creates/updates/deletes seat types. Public GET endpoint is in SeatTypesController.
    /// </summary>
    [ApiController]
    [Route("api/admin/seat-types")]
    // [Authorize(Roles = "Admin")]
    public class AdminSeatTypesController : BaseApiController
    {
        /// <summary>
        /// Create a new seat type — e.g., "VIP", "Standard", "Couple".
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] Shared.Models.DataModels.SharedDtos.CreateSeatTypeRequest request)
        {
            return Ok(await Mediator.Send(new Application.Features.Shared.SeatTypes.Commands.Create.CreateSeatTypeCommand(request)));
        }

        /// <summary>
        /// Update seat type.
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Shared.Models.DataModels.SharedDtos.UpdateSeatTypeRequest request)
        {
            await Mediator.Send(new Application.Features.Shared.SeatTypes.Commands.Update.UpdateSeatTypeCommand(id, request));
            return NoContent();
        }

        /// <summary>
        /// Delete seat type — soft delete (deactivate).
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await Mediator.Send(new DeleteSeatTypeCommand(id));
            return NoContent();
        }
    }

    public record DeleteSeatTypeCommand(Guid Id) : IRequest;
}
