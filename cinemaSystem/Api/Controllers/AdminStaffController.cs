using Application.Features.Staff.Commands.AssignStaff;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DataModels.StaffDtos;
using System;
using System.Threading.Tasks;

namespace Api.Controllers
{
    // [Authorize(Roles = "SuperAdmin,Admin")]
    [Route("api/admin/staff")]
    public class AdminStaffController : BaseApiController
    {
        [HttpPost("assignments")]
        public async Task<ActionResult<Guid>> AssignStaff([FromBody] StaffAssignmentRequest request)
        {
            return Ok(await Mediator.Send(new AssignStaffCommand(request)));
        }
    }
}
