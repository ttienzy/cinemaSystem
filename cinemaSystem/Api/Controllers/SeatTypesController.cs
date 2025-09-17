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
    public class SeatTypesController : ControllerBase
    {
        private readonly ISeatTypeService _seatTypeService;
        public SeatTypesController(ISeatTypeService seatTypeService)
        {
            _seatTypeService = seatTypeService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var response = await _seatTypeService.GetSeatTypesAsync();
            if (response.IsSuccess)
            {
                return Ok(response.Value);
            }
            return ErrorResponse<IEnumerable<SeatType>>.WithError(response);
        }
        [HttpGet("{seatTypeId}")]
        public async Task<IActionResult> GetByIdAsync(Guid seatTypeId)
        {
            var response = await _seatTypeService.GetSeatTypeByIdAsync(seatTypeId);
            if (response.IsSuccess)
            {
                return Ok(response.Value);
            }
            return ErrorResponse<SeatType>.WithError(response);
        }
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] SeatTypeRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request data.");
            }
            var response = await _seatTypeService.CreateSeatTypeAsync(request);
            if (response.IsSuccess)
            {
                return Ok(response.Value);
            }
            return ErrorResponse<SeatType>.WithError(response);
        }
        [HttpPut("{seatTypeId}")]
        public async Task<IActionResult> UpdateAsync(Guid seatTypeId, [FromBody] SeatTypeRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request data.");
            }
            var response = await _seatTypeService.UpdateSeatTypeAsync(seatTypeId, request);
            if (response.IsSuccess)
            {
                return Ok(response.Value);
            }
            return ErrorResponse<SeatType>.WithError(response);
        }
        [HttpDelete("{seatTypeId}")]
        public async Task<IActionResult> DeleteAsync(Guid seatTypeId)
        {
            var response = await _seatTypeService.DeleteSeatTypeAsync(seatTypeId);
            if (response.IsSuccess)
            {
                return NoContent();
            }
            return ErrorResponse<object>.WithError(response);
        }
    }
}
