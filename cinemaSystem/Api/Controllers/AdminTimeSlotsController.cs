using Application.Features.Shared.TimeSlots.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// Time Slot management — Admin only.
    /// Admin creates/updates/deletes movie showtime slots (Morning, Afternoon, Evening, Late night).
    /// </summary>
    [ApiController]
    [Route("api/admin/time-slots")]
    // [Authorize(Roles = "Admin")]
    public class AdminTimeSlotsController(IMediator mediator) : ControllerBase
    {
        /// <summary>
        /// Create a new time slot — e.g., "Prime time 18:00–21:00".
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] TimeSlotRequest request)
        {
            var id = await mediator.Send(new CreateTimeSlotCommand(request.StartTime, request.EndTime, request.dateType));
            return Ok(new { id });
        }

        /// <summary>
        /// Update time slot — change name, start/end times.
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] TimeSlotRequest request)
        {
            await mediator.Send(new UpdateTimeSlotCommand(id, request.Name, request.StartTime, request.EndTime, request.dateType, request.isActive));
            return NoContent();
        }

        /// <summary>
        /// Delete time slot.
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await mediator.Send(new DeleteTimeSlotCommand(id));
            return NoContent();
        }
    }

    /// <summary>Request body for TimeSlot CUD.</summary>
    public record TimeSlotRequest(string Name, TimeSpan StartTime, TimeSpan EndTime, string dateType, bool isActive = true);
}
