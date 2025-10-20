using Application.Interfaces.Persistences;
using Azure;
using Domain.Entities.StaffAggregate;
using Infrastructure.Data.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Base;
using Shared.Models.DataModels.InventoryDtos;
using Shared.Models.DataModels.StaffDtos;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StaffsController : ControllerBase
    {
        private readonly IStaffManager _staffManager;
        public StaffsController(IStaffManager staffManager)
        {
            _staffManager = staffManager;
        }

        [HttpGet("schedules/{cinemaId:guid}")]
        public async Task<IActionResult> GetStaffInfoAsync(Guid cinemaId, [FromQuery] DateTime? ShiftDate)
        {
            var date = ShiftDate ?? DateTime.UtcNow;
            var result = await _staffManager.GetStaffInfoAsync(cinemaId, date);
            if (result.IsSuccess)
                return Ok(result.Value);
            return ErrorResponse<IEnumerable<StaffInfoResponse>>.WithError(result);
        }
        [HttpGet("staff-on-time")]
        public async Task<IActionResult> GetStaffOnTimeAsync([FromQuery] string email)
        {
            var result = await _staffManager.GetStaffOnTimeAsync(email);
            if (result.IsSuccess)
                return Ok(result.Value);
            return ErrorResponse<StaffReponse>.WithError(result);
        }
        [HttpGet("{cinemaId:guid}/shifts")]
        public async Task<IActionResult> GetShiftsAsync(Guid cinemaId)
        {
            var result = await _staffManager.GetShiftsInfoAsync(cinemaId);
            if (result.IsSuccess)
                return Ok(result.Value);
            return ErrorResponse<IEnumerable<ShiftInfoResponse>>.WithError(result);
        }
        [HttpGet("staffs-manager/{cinemaId:guid}")]
        public async Task<IActionResult> GetStaffsAsync(Guid cinemaId)
        {
            var result = await _staffManager.GetStaffToCinemaAsync(cinemaId);
            if (result.IsSuccess)
                return Ok(result.Value);
            return ErrorResponse<IEnumerable<GetStaffToCinemaResponse>>.WithError(result);
        }
        [HttpGet("shifts-manager/{cinemaId:guid}")]
        public async Task<IActionResult> GetShiftsMnAsync(Guid cinemaId)
        {
            var result = await _staffManager.GetShiftsMnAsync(cinemaId);
            if (result.IsSuccess)
                return Ok(result.Value);
            return ErrorResponse<IEnumerable<GetShiftsToCinemaResponse>>.WithError(result);
        }
        [HttpPost("add-staff-to-cinema")]
        public async Task<IActionResult> AddStaffToCinemaAsync([FromBody] EmployeeCreateRequest request)
        {
            var result = await _staffManager.AddStaffToCinemaAsync(request);
            if (result.IsSuccess)
                return Ok(result.Value);
            return ErrorResponse<string>.WithError(result);
        }
        [HttpPost("add-shifts-to-cinema")]
        public async Task<IActionResult> AddShiftsToCinema([FromBody] IEnumerable<ShiftRequest> requests)
        {
            var results = await _staffManager.AddShiftsToCinemaAsync(requests);
            if (results.IsSuccess)
                return Ok(results.Value);
            return ErrorResponse<string>.WithError(results);
        }
        [HttpPost("add-shift-to-employee")]
        public async Task<IActionResult> AddShiftToEmployee([FromBody] TakeAttendanceOEmpRequest request)
        {
            var results = await _staffManager.AddShiftToEmployeeAsync(request);
            if (results.IsSuccess)
                return Ok(results.Value);
            return ErrorResponse<object>.WithError(results);
        }
    }
}
