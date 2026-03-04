using Application.Features.Movies.Commands.CreateMovie;
using Application.Features.Movies.Commands.UpdateMovie;
using Application.Features.Movies.Commands.DeleteMovie;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DataModels.MovieDtos;

namespace Api.Controllers
{
    /// <summary>
    /// Handles admin movie management APIs.
    /// </summary>
    public class AdminMoviesController : BaseApiController
    {
        /// <summary>
        /// Create a new movie.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Guid>> CreateMovie([FromBody] MovieUpsertRequest request)
        {
            return Ok(await Mediator.Send(new CreateMovieCommand(request)));
        }

        /// <summary>
        /// Update an existing movie.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMovie(Guid id, [FromBody] MovieUpsertRequest request)
        {
            await Mediator.Send(new UpdateMovieCommand(id, request));
            return NoContent();
        }

        /// <summary>
        /// Delete a movie.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMovie(Guid id)
        {
            await Mediator.Send(new DeleteMovieCommand(id));
            return NoContent();
        }
    }
}
