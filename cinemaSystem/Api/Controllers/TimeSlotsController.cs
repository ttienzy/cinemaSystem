using Application.Interfaces.Persistences;
using Domain.Entities.SharedAggregates;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Base;
using Shared.Models.DataModels.ClassificationDtos;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimeSlotsController : ControllerBase
    {
        private readonly ITimeSlotService _timeSlotService;
        public TimeSlotsController(ITimeSlotService timeSlotService)
        {
            _timeSlotService = timeSlotService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTimeSlots()
        {
            var result = await _timeSlotService.GetTimeSlotsAsync();
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            return ErrorResponse<IEnumerable<TimeSlot>>.WithError(result);
        }
        [HttpGet("{timeSlotId:guid}")]
        public async Task<IActionResult> GetTimeSlotById(Guid timeSlotId)
        {
            var result = await _timeSlotService.GetTimeSlotByIdAsync(timeSlotId);
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            return ErrorResponse<TimeSlot>.WithError(result);
        }
        [HttpPost]
        public async Task<IActionResult> CreateTimeSlot([FromBody] TimeSlotRequest request)
        {
            if (request == null)
            {
                return BadRequest("Time slot request cannot be null.");
            }
            var result = await _timeSlotService.CreateTimeSlotAsync(request);
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            return ErrorResponse<TimeSlot>.WithError(result);
        }
        [HttpPut("{timeSlotId:guid}")]
        public async Task<IActionResult> UpdateTimeSlot(Guid timeSlotId, [FromBody] TimeSlotRequest request)
        {
            if (request == null)
            {
                return BadRequest("Time slot request cannot be null.");
            }
            var result = await _timeSlotService.UpdateTimeSlotAsync(timeSlotId, request);
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            return ErrorResponse<TimeSlot>.WithError(result);
        }
        [HttpDelete("{timeSlotId:guid}")]
        public async Task<IActionResult> DeleteTimeSlot(Guid timeSlotId)
        {
            var result = await _timeSlotService.DeleteTimeSlotAsync(timeSlotId);
            if (result.IsSuccess)
            {
                return Ok();
            }
            return ErrorResponse<object>.WithError(result);
        }
    }
}
