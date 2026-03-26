using Application.Common.Interfaces.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.IdentityModels;
using System.Security.Claims;

namespace Api.Controllers
{
    /// <summary>
    /// Personal Profile — For logged-in Customers.
    /// Separated from IdentityController to maintain Single Responsibility.
    /// Automatically retrieves userId from JWT token, no need to pass it in the URL.
    /// </summary>
    [ApiController]
    [Route("api/profile")]
    // [Authorize]
    public class ProfileController(IIdentityUserService identityService) : ControllerBase
    {
        /// <summary>
        /// View personal profile — retrieves userId from token.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<UserProfileResponse>> GetMyProfile()
        {
            var userId = GetCurrentUserId();
            return Ok(await identityService.GetUserProfileAsync(userId));
        }

        /// <summary>
        /// Update personal profile — only allows editing one's own information.
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = GetCurrentUserId();
            await identityService.UpdateProfileAsync(userId, request);
            return Ok(new { message = "Profile updated successfully." });
        }

        /// <summary>
        /// Change password — requires old password + new password.
        /// </summary>
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            await identityService.ChangePasswordAsync(request);
            return Ok(new { message = "Password changed successfully." });
        }

        private Guid GetCurrentUserId()
            => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
