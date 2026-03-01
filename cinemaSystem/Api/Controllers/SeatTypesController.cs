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
    // [Authorize(Roles = "Admin,Manager")]
    public class SeatTypesController : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<List<SeatTypeDto>>> GetAll()
        {
            return Ok(await Mediator.Send(new GetAllSeatTypesQuery()));
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateSeatTypeRequest request)
        {
            return Ok(await Mediator.Send(new CreateSeatTypeCommand(request)));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSeatTypeRequest request)
        {
            await Mediator.Send(new UpdateSeatTypeCommand(id, request));
            return NoContent();
        }
    }
}
