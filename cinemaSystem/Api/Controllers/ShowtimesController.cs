using Application.Interfaces.Persistences;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Base;
using Shared.Models.DataModels.ShowtimeDtos;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShowtimesController : ControllerBase
    {
        private readonly IShowtimeService _showtimeService;
        public ShowtimesController(IShowtimeService showtimeService)
        {
            _showtimeService = showtimeService;
        }


        [HttpGet]
        public async Task<IActionResult> GetShowtimes([FromQuery] ShowtimeQueryParameters parameters)
        {
            var result = await _showtimeService.GetShowtimesAsync(parameters);
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            return ErrorResponse<IEnumerable<ShowtimeFeaturedResponse>>.WithError(result);
        }
        [HttpGet("{showtimeId}/seating-plan")]
        public async Task<IActionResult> GetShowtimeSeatingPlan(Guid showtimeId)
        {
            var result = await _showtimeService.GetShowtimeSeatingPlanAsync(showtimeId);
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            return ErrorResponse<ShowtimeSeatingPlanResponse>.WithError(result);
        }
        [HttpGet("detail")]
        public async Task<IActionResult> GetShowtimeFeaturedAsync([FromQuery] ShowtimeQueryParameters parameters)
        {
            var result = await _showtimeService.GetShowtimeFeaturedAsync(parameters);
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            return ErrorResponse<ShowtimeFeaturedResponse>.WithError(result);
        }
        [HttpPost]
        public async Task<IActionResult> CreateShowtime([FromBody] ShowtimeRequest request)
        {
            var result = await _showtimeService.CreateShowtimeAsync(request);
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            return ErrorResponse<ShowtimeResponse>.WithError(result);
        }
        [HttpPost("{showtimeId}/pricings")]
        public async Task<IActionResult> AddPricingToShowtime(Guid showtimeId, [FromBody] ShowtimePricingRequest request)
        {
            var result = await _showtimeService.AddPricingToShowtimeAsync(showtimeId, request);
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            return ErrorResponse<ShowtimePricingResponse>.WithError(result);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShowtime(Guid id, [FromBody] ShowtimeRequest request)
        {
            var result = await _showtimeService.UpdateShowtimeAsync(id, request);
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            return ErrorResponse<ShowtimeResponse>.WithError(result);
        }
        [HttpPut("{showtimeId}/pricings/{pricingId}")]
        public async Task<IActionResult> UpdatePricingToShowtime(Guid showtimeId, Guid pricingId, [FromBody] ShowtimePricingRequest request)
        {
            var result = await _showtimeService.UpdatePricingToShowtimeAsync(showtimeId, pricingId, request);
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            return ErrorResponse<ShowtimePricingResponse>.WithError(result);
        }
        [HttpDelete("{showtimeId}")]
        public async Task<IActionResult> DeleteShowtime(Guid showtimeId)
        {
            var result = await _showtimeService.DeleteShowtimeAsync(showtimeId);
            if (result.IsSuccess)
            {
                return Ok(new { message = "Showtime deleted successfully." });
            }
            return ErrorResponse<object>.WithError(result);
        }
        [HttpDelete("{showtimeId}/pricings/{pricingId}")]
        public async Task<IActionResult> DeletePricingFromShowtime(Guid showtimeId, Guid pricingId)
        {
            var result = await _showtimeService.DeletePricingFromShowtimeAsync(showtimeId, pricingId);
            if (result.IsSuccess)
            {
                return Ok(new { message = "Pricing deleted from showtime successfully." });
            }
            return ErrorResponse<object>.WithError(result);
        }
    }
}
