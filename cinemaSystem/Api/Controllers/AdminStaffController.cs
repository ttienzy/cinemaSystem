using Application.Features.Staff.Commands.AssignStaff;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DataModels.StaffDtos;

namespace Api.Controllers
{
    /// <summary>
    /// Handles admin staff management APIs.
    /// </summary>
    [Route("api/admin/staff")]
    public class AdminStaffController : BaseApiController
    {
        /// <summary>
        /// Assign staff to a cinema.
        /// </summary>
        [HttpPost("assignments")]
        public async Task<ActionResult<Guid>> AssignStaff([FromBody] StaffAssignmentRequest request)
        {
            return Ok(await Mediator.Send(new AssignStaffCommand(request)));
        }
    }
}
