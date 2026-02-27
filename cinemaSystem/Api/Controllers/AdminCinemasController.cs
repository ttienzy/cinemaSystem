using Application.Features.Cinemas.Commands.CreateCinema;
using Application.Features.Cinemas.Commands.UpdateCinema;
using Application.Features.Cinemas.Commands.DeleteCinema;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DataModels.CinemaDtos;

namespace Api.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminCinemasController : BaseApiController
    {
        [HttpPost]
        public async Task<ActionResult<Guid>> CreateCinema([FromBody] CinemaUpsertRequest request)
        {
            return Ok(await Mediator.Send(new CreateCinemaCommand(request)));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCinema(Guid id, [FromBody] CinemaUpsertRequest request)
        {
            await Mediator.Send(new UpdateCinemaCommand(id, request));
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCinema(Guid id)
        {
            await Mediator.Send(new DeleteCinemaCommand(id));
            return NoContent();
        }
    }
}
