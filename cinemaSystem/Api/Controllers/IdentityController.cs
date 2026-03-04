using Application.Common.Interfaces.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.IdentityModels;
using Shared.Models.IdentityModels.Otps;

namespace Api.Controllers
{
    /// <summary>
    /// Handles user profile and identity management APIs.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController(IIdentityUserService identityService) : ControllerBase
    {
        /// <summary>
        /// Get user profile by user ID.
        /// </summary>
        [HttpGet("profile/{userId}")]
        public async Task<ActionResult<UserProfileResponse>> GetProfile(Guid userId)
        {
            return Ok(await identityService.GetUserProfileAsync(userId));
        }

        /// <summary>
        /// Update user profile information.
        /// </summary>
        [HttpPut("profile/{userId}")]
        public async Task<IActionResult> UpdateProfile(Guid userId, [FromBody] UpdateProfileRequest request)
        {
            await identityService.UpdateProfileAsync(userId, request);
            return Ok("Profile updated successfully.");
        }

        /// <summary>
        /// Change user password.
        /// </summary>
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            await identityService.ChangePasswordAsync(request);
            return Ok("Password changed successfully.");
        }

        [AllowAnonymous]
        /// <summary>
        /// Request password reset OTP via email.
        /// </summary>
        [HttpPost("forgot-password-with-otp")]
        public async Task<IActionResult> ForgotPassword([FromBody] string email)
        {
            await identityService.ForgotPasswordAsync(email);
            return Ok("Verification code sent to your email.");
        }

        [AllowAnonymous]
        /// <summary>
        /// Verify OTP for password reset.
        /// </summary>
        [HttpPost("verify-reset-otp")]
        public async Task<IActionResult> VerifyResetOtp([FromBody] VerifyResetOtpRequest request)
        {
            await identityService.VerifyResetOtpAsync(request);
            return Ok("OTP verified successfully.");
        }

        [AllowAnonymous]
        /// <summary>
        /// Reset password using verified OTP.
        /// </summary>
        [HttpPost("reset-password-with-otp")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordWithOtpRequest request)
        {
            await identityService.ResetPasswordAsync(request);
            return Ok("Password reset successfully.");
        }

        /// <summary>
        /// Create a new staff account.
        /// </summary>
        [HttpPost("staff")]
        public async Task<IActionResult> CreateStaff([FromBody] CreateStaffRequest request)
        {
            await identityService.CreateStaffAsync(request);
            return Ok("Staff account created successfully and welcome email sent.");
        }

        /// <summary>
        /// Update user role.
        /// </summary>
        [HttpPut("users/{userId}/role")]
        public async Task<IActionResult> UpdateUserRole(Guid userId, [FromBody] UpdateUserRoleRequest request)
        {
            await identityService.UpdateUserRoleAsync(userId, request);
            return Ok("User role updated successfully.");
        }
    }
}
