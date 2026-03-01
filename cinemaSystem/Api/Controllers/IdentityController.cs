using Application.Common.Interfaces.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.IdentityModels;
using Shared.Models.IdentityModels.Otps;

using Infrastructure.Identity.Constants;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController(IIdentityUserService identityService) : ControllerBase
    {
        // [Authorize]
        [HttpGet("profile/{userId}")]
        public async Task<ActionResult<UserProfileResponse>> GetProfile(Guid userId)
        {
            return Ok(await identityService.GetUserProfileAsync(userId));
        }

        //[Authorize]
        [HttpPut("profile/{userId}")]
        public async Task<IActionResult> UpdateProfile(Guid userId, [FromBody] UpdateProfileRequest request)
        {
            await identityService.UpdateProfileAsync(userId, request);
            return Ok("Profile updated successfully.");
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            await identityService.ChangePasswordAsync(request);
            return Ok("Password changed successfully.");
        }

        [HttpPost("forgot-password-with-otp")]
        public async Task<IActionResult> ForgotPassword([FromBody] string email)
        {
            await identityService.ForgotPasswordAsync(email);
            return Ok("Verification code sent to your email.");
        }

        [HttpPost("verify-reset-otp")]
        public async Task<IActionResult> VerifyResetOtp([FromBody] VerifyResetOtpRequest request)
        {
            await identityService.VerifyResetOtpAsync(request);
            return Ok("OTP verified successfully.");
        }

        [HttpPost("reset-password-with-otp")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordWithOtpRequest request)
        {
            await identityService.ResetPasswordAsync(request);
            return Ok("Password reset successfully.");
        }

        //[Authorize(Roles = RoleConstant.SuperAdmin + "," + RoleConstant.Admin)]
        [HttpPost("staff")]
        public async Task<IActionResult> CreateStaff([FromBody] CreateStaffRequest request)
        {
            await identityService.CreateStaffAsync(request);
            return Ok("Staff account created successfully and welcome email sent.");
        }

        //[Authorize(Roles = RoleConstant.SuperAdmin + "," + RoleConstant.Admin)]
        [HttpPut("users/{userId}/role")]
        public async Task<IActionResult> UpdateUserRole(Guid userId, [FromBody] UpdateUserRoleRequest request)
        {
            await identityService.UpdateUserRoleAsync(userId, request);
            return Ok("User role updated successfully.");
        }
    }
}
