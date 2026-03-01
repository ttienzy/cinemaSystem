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
    // [Authorize(Roles = "Admin,Manager")]
    public class GenresController : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<List<GenreDto>>> GetAll([FromQuery] bool activeOnly = false)
        {
            return Ok(await Mediator.Send(new GetAllGenresQuery(activeOnly)));
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateGenreRequest request)
        {
            return Ok(await Mediator.Send(new CreateGenreCommand(request)));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGenreRequest request)
        {
            await Mediator.Send(new UpdateGenreCommand(id, request));
            return NoContent();
        }
    }
}
