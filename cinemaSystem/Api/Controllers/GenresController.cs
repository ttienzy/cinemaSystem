using Application.Features.Shared.Genres.Commands.Create;
using Application.Features.Shared.Genres.Commands.Update;
using Application.Features.Shared.Genres.Queries.GetAll;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DataModels.SharedDtos;

namespace Api.Controllers
{
    /// <summary>
    /// Handles genre management APIs.
    /// </summary>
    public class GenresController : BaseApiController
    {
        /// <summary>
        /// Get all genres.
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<GenreDto>>> GetAll([FromQuery] bool activeOnly = false)
        {
            return Ok(await Mediator.Send(new GetAllGenresQuery(activeOnly)));
        }

        /// <summary>
        /// Create a new genre.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateGenreRequest request)
        {
            return Ok(await Mediator.Send(new CreateGenreCommand(request)));
        }

        /// <summary>
        /// Update a genre.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGenreRequest request)
        {
            await Mediator.Send(new UpdateGenreCommand(id, request));
            return NoContent();
        }
    }
}
