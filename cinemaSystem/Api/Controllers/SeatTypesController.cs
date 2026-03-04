using Application.Features.Shared.SeatTypes.Commands.Create;
using Application.Features.Shared.SeatTypes.Commands.Update;
using Application.Features.Shared.SeatTypes.Queries.GetAll;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DataModels.SharedDtos;

namespace Api.Controllers
{
    /// <summary>
    /// Handles seat type management APIs.
    /// </summary>
    public class SeatTypesController : BaseApiController
    {
        /// <summary>
        /// Get all seat types.
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<SeatTypeDto>>> GetAll()
        {
            return Ok(await Mediator.Send(new GetAllSeatTypesQuery()));
        }

        /// <summary>
        /// Create a new seat type.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateSeatTypeRequest request)
        {
            return Ok(await Mediator.Send(new CreateSeatTypeCommand(request)));
        }

        /// <summary>
        /// Update a seat type.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSeatTypeRequest request)
        {
            await Mediator.Send(new UpdateSeatTypeCommand(id, request));
            return NoContent();
        }
    }
}
