using Application.Features.Shifts.Commands.CreateShift;
using Application.Features.Shifts.Commands.UpdateShift;
using Application.Features.Shifts.Commands.DeleteShift;
using Application.Features.Shifts.Queries.GetShiftsByCinema;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DataModels.StaffDtos;

namespace Api.Controllers
{
    /// <summary>
    /// Shift management — For Managers.
    /// Each cinema can have multiple shifts: "Morning Shift 08:00–14:00", "Afternoon Shift 14:00–22:00".
    /// Managers create shifts, then assign staff to shifts via ManagerSchedulesController.
    /// </summary>
    [ApiController]
    [Route("api/manager/shifts")]
    // [Authorize(Roles = "Manager,Admin")]
    public class ManagerShiftsController(IMediator mediator) : ControllerBase
    {
        /// <summary>
        /// Get list of shifts by cinema — used for dropdowns when scheduling.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<ShiftDto>>> GetShifts([FromQuery] Guid cinemaId)
        {
            return Ok(await mediator.Send(new GetShiftsByCinemaQuery(cinemaId)));
        }

        /// <summary>
        /// Create a new shift — e.g., "Morning Shift" from 08:00 to 14:00.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Guid>> CreateShift([FromBody] ShiftUpsertRequest request)
        {
            var id = await mediator.Send(new CreateShiftCommand(request));
            return CreatedAtAction(nameof(GetShifts), new { cinemaId = request.CinemaId }, new { id });
        }

        /// <summary>
        /// Update shift — change start/end times or shift name.
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateShift(Guid id, [FromBody] ShiftUpsertRequest request)
        {
            await mediator.Send(new UpdateShiftCommand(id, request));
            return NoContent();
        }

        /// <summary>
        /// Delete shift — cannot delete if there are future schedule assignments.
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteShift(Guid id)
        {
            await mediator.Send(new DeleteShiftCommand(id));
            return NoContent();
        }
    }
}
