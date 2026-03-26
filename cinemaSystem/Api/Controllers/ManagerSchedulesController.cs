using Application.Features.Schedules.Commands.CreateSchedule;
using Application.Features.Schedules.Commands.BulkCreateSchedule;
using Application.Features.Schedules.Queries.GetWeeklySchedule;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DataModels.StaffDtos;

namespace Api.Controllers
{
    /// <summary>
    /// Employee schedule management — For Managers.
    /// Managers assign staff to shifts by day, supports bulk scheduling for the whole week.
    /// View weekly schedule in table format: staff x day x shift.
    /// </summary>
    [ApiController]
    [Route("api/manager/schedules")]
    // [Authorize(Roles = "Manager,Admin")]
    public class ManagerSchedulesController(IMediator mediator) : ControllerBase
    {
        /// <summary>
        /// View weekly schedule — pass any day of the week, the system automatically calculates Monday.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<WorkScheduleDto>>> GetWeeklySchedule(
            [FromQuery] Guid cinemaId,
            [FromQuery] DateTime weekOf)
        {
            return Ok(await mediator.Send(new GetWeeklyScheduleQuery(cinemaId, weekOf)));
        }

        /// <summary>
        /// Assign staff to a specific shift — validates no duplicate days.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Guid>> CreateSchedule([FromBody] ScheduleCreateRequest request)
        {
            var id = await mediator.Send(new CreateScheduleCommand(request));
            return Ok(new { id });
        }

        /// <summary>
        /// Bulk scheduling — send multiple entries at once (full week for multiple staff).
        /// Returns success/skipped count and error list.
        /// </summary>
        [HttpPost("bulk")]
        public async Task<IActionResult> BulkCreate([FromBody] BulkScheduleRequest request)
        {
            var result = await mediator.Send(new BulkCreateScheduleCommand(request));
            return Ok(result);
        }
    }
}
