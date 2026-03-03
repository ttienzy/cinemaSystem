using Application.Features.Movies.Commands.CreateMovie;
using Application.Features.Movies.Commands.UpdateMovie;
using Application.Features.Movies.Commands.DeleteMovie;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DataModels.MovieDtos;

namespace Api.Controllers
{
    // [Authorize(Roles = "Admin,Manager")]
    public class AdminMoviesController : BaseApiController
    {
        [HttpPost]
        public async Task<ActionResult<Guid>> CreateMovie([FromBody] MovieUpsertRequest request)
        {
            return Ok(await Mediator.Send(new CreateMovieCommand(request)));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMovie(Guid id, [FromBody] MovieUpsertRequest request)
        {
            await Mediator.Send(new UpdateMovieCommand(id, request));
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMovie(Guid id)
        {
            await Mediator.Send(new DeleteMovieCommand(id));
            return NoContent();
        }
    }
}
