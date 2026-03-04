using Application.Features.Shared.TimeSlots.Queries.GetAll;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DataModels.SharedDtos;

namespace Api.Controllers
{
    /// <summary>
    /// Handles time slot management APIs.
    /// </summary>
    public class TimeSlotsController : BaseApiController
    {
        /// <summary>
        /// Get all time slots.
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<TimeSlotDto>>> GetAll([FromQuery] bool activeOnly = false)
        {
            return Ok(await Mediator.Send(new GetAllTimeSlotsQuery(activeOnly)));
        }
    }
}
