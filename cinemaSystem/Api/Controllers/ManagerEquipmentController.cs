using Application.Features.Equipment.Commands;
using Application.Features.Equipment.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DataModels.EquipmentDtos;

namespace Api.Controllers
{
    /// <summary>
    /// Quản lý thiết bị tại rạp — Dành cho Manager.
    /// </summary>
    [ApiController]
    [Route("api/manager/equipment")]
    [Authorize(Roles = "Manager,Admin")]
    public class ManagerEquipmentController(IMediator mediator) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<EquipmentResponse>>> GetByCinema(
            [FromQuery] Guid cinemaId, [FromQuery] string? status = null)
        {
            return Ok(await mediator.Send(new GetEquipmentListQuery(cinemaId, status)));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<EquipmentResponse>> GetById(Guid id)
        {
            return Ok(await mediator.Send(new GetEquipmentByIdQuery(id)));
        }

        [HttpPost("{id:guid}/maintenance")]
        public async Task<ActionResult<Guid>> CreateMaintenanceLog(
            Guid id, [FromBody] MaintenanceLogRequest request)
        {
            var logId = await mediator.Send(new CreateMaintenanceLogCommand(
                id, request.MaintenanceDate, request.Cost, request.IssuesFound, request.IsCompleted));
            return Ok(new { id = logId });
        }

        [HttpGet("{id:guid}/maintenance-logs")]
        public async Task<ActionResult<List<MaintenanceLogResponse>>> GetMaintenanceLogs(Guid id)
        {
            return Ok(await mediator.Send(new GetMaintenanceLogsQuery(id)));
        }
    }
}
