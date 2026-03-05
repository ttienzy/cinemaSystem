using Application.Features.Equipment.Commands;
using Application.Features.Equipment.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DataModels.EquipmentDtos;

namespace Api.Controllers
{
    /// <summary>
    /// Quản lý thiết bị — Chỉ dành cho Admin.
    /// </summary>
    [ApiController]
    [Route("api/admin/equipment")]
    [Authorize(Roles = "Admin")]
    public class AdminEquipmentController(IMediator mediator) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<EquipmentResponse>>> GetAll([FromQuery] Guid? cinemaId = null, [FromQuery] string? status = null)
        {
            return Ok(await mediator.Send(new GetEquipmentListQuery(cinemaId, status)));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<EquipmentResponse>> GetById(Guid id)
        {
            return Ok(await mediator.Send(new GetEquipmentByIdQuery(id)));
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] EquipmentRequest request)
        {
            var id = await mediator.Send(new CreateEquipmentCommand(
                request.CinemaId, request.ScreenId, request.EquipmentType, request.PurchaseDate, request.Status));
            return Ok(new { id });
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] EquipmentRequest request)
        {
            await mediator.Send(new UpdateEquipmentCommand(id, request.CinemaId, request.ScreenId, request.EquipmentType, request.Status));
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await mediator.Send(new DeleteEquipmentCommand(id));
            return NoContent();
        }
    }
}
