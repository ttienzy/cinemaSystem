using Application.Common.Interfaces.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.IdentityModels;

namespace Api.Controllers
{
    /// <summary>
    /// System user management — Admin only.
    /// Includes: View user list, user details, lock/unlock accounts,
    /// create staff accounts, and update roles.
    /// </summary>
    [ApiController]
    [Route("api/admin/users")]
    // [Authorize(Roles = "Admin")]
    public class AdminUsersController(
        IIdentityUserService identityService,
        IUserManagementService userManagementService) : ControllerBase
    {
        /// <summary>
        /// Get list of all users (paginated, filter by role/status/keyword).
        /// Used for Admin's user management page.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllUsers(
            [FromQuery] string? role = null,
            [FromQuery] string? search = null,
            [FromQuery] bool? isLocked = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await userManagementService.GetAllUsersAsync(role, search, isLocked, page, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// View detailed information for a user (profile + role + status).
        /// </summary>
        [HttpGet("{userId:guid}")]
        public async Task<ActionResult<UserProfileResponse>> GetUserById(Guid userId)
        {
            return Ok(await identityService.GetUserProfileAsync(userId));
        }

        /// <summary>
        /// Lock user account — prevents login.
        /// Used for unusual activity or policy violations.
        /// </summary>
        [HttpPut("{userId:guid}/lock")]
        public async Task<IActionResult> LockUser(Guid userId)
        {
            await userManagementService.LockUserAsync(userId);
            return Ok(new { message = "Account locked." });
        }

        /// <summary>
        /// Unlock user account — allows login again.
        /// </summary>
        [HttpPut("{userId:guid}/unlock")]
        public async Task<IActionResult> UnlockUser(Guid userId)
        {
            await userManagementService.UnlockUserAsync(userId);
            return Ok(new { message = "Account unlocked." });
        }

        /// <summary>
        /// Create a new staff account — sends welcome email automatically.
        /// (Moved from IdentityController for proper Admin authorization.)
        /// </summary>
        [HttpPost("staff")]
        public async Task<IActionResult> CreateStaff([FromBody] CreateStaffRequest request)
        {
            await identityService.CreateStaffAsync(request);
            return Ok(new { message = "Staff account created successfully." });
        }

        /// <summary>
        /// Update user roles.
        /// E.g., promoting Staff to Manager, or demoting Manager to Staff.
        /// </summary>
        [HttpPut("{userId:guid}/role")]
        public async Task<IActionResult> UpdateUserRole(Guid userId, [FromBody] UpdateUserRoleRequest request)
        {
            await identityService.UpdateUserRoleAsync(userId, request);
            return Ok(new { message = "Roles updated successfully." });
        }
    }
}
