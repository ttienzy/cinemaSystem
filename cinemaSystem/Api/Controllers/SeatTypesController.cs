using Application.Features.Shared.SeatTypes.Commands.Create;
using Application.Features.Shared.SeatTypes.Commands.Update;
using Application.Features.Shared.SeatTypes.Queries.GetAll;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DataModels.SharedDtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Controllers
{
    public class SeatTypesController : BaseApiController
    {
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<SeatTypeDto>>> GetAll()
        {
            return Ok(await Mediator.Send(new GetAllSeatTypesQuery()));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateSeatTypeRequest request)
        {
            return Ok(await Mediator.Send(new CreateSeatTypeCommand(request)));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSeatTypeRequest request)
        {
            await Mediator.Send(new UpdateSeatTypeCommand(id, request));
            return NoContent();
        }
    }
}
