using Application.Features.Shared.Genres.Commands.Create;
using Application.Features.Shared.Genres.Commands.Update;
using Application.Features.Shared.Genres.Queries.GetAll;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DataModels.SharedDtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Controllers
{
    public class GenresController : BaseApiController
    {
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<GenreDto>>> GetAll([FromQuery] bool activeOnly = false)
        {
            return Ok(await Mediator.Send(new GetAllGenresQuery(activeOnly)));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateGenreRequest request)
        {
            return Ok(await Mediator.Send(new CreateGenreCommand(request)));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGenreRequest request)
        {
            await Mediator.Send(new UpdateGenreCommand(id, request));
            return NoContent();
        }
    }
}
