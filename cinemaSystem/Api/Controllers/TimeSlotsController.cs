using Application.Features.Shared.TimeSlots.Queries.GetAll;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DataModels.SharedDtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Controllers
{
    // [Authorize(Roles = "Admin,Manager")]
    public class TimeSlotsController : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<List<TimeSlotDto>>> GetAll([FromQuery] bool activeOnly = false)
        {
            return Ok(await Mediator.Send(new GetAllTimeSlotsQuery(activeOnly)));
        }
        
        // POST and PUT can be added later if needed. Usually timeslots are static.
    }
}
