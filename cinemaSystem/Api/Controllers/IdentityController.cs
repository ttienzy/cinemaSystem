using Application.Interfaces.Persistences;
using Application.Interfaces.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Base;
using Shared.Models.IdentityModels;
using Shared.Models.IdentityModels.Otps;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private readonly IIdentityUserService _identityService;
        public IdentityController(IIdentityUserService identityService)
        {
            _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        }
        // User Profile Operations
        [Authorize]
        [HttpGet("profile/{userId}")]
        public async Task<IActionResult> GetUserProfileAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest("User ID cannot be empty.");
            }
            var response = await _identityService.GetUserProfileAsync(userId);
            if (response.IsSuccess)
            {
                return Ok(response.Value);
            }
            return ErrorResponse<UserProfileResponse>.WithError(response);
        }
        [HttpPut("profile/{userId}")]
        public async Task<IActionResult> UpdateProfileAsync(Guid userId, [FromBody] UpdateProfileRequest updateProfileRequest)
        {
            if (userId == Guid.Empty || updateProfileRequest == null)
            {
                return BadRequest("User ID and update profile request cannot be null.");
            }
            var response = await _identityService.UpdateProfileAsync(userId, updateProfileRequest);
            if (response.IsSuccess)
            {
                return Ok(response.Value);
            }
            return ErrorResponse<string>.WithError(response);
        }



        // OTP Reset Operations and Password Reset Operations
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordRequest changePasswordRequest)
        {
            if (changePasswordRequest == null)
            {
                return BadRequest("Change password request cannot be null.");
            }
            var response = await _identityService.ChangePasswordAsync(changePasswordRequest);
            if (response.IsSuccess)
            {
                return Ok(response.Value);
            }
            return ErrorResponse<string>.WithError(response);
        }
        [HttpPost("forgot-password-with-otp")]
        public async Task<IActionResult> ForgotPasswordAsync([FromBody] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email cannot be null or empty.");
            }
            var response = await _identityService.ForgotPasswordAsync(email);
            if (response.IsSuccess)
            {
                return Ok(response.Value);
            }
            return ErrorResponse<string>.WithError(response);
        }
        [HttpPost("verify-reset-otp")]
        public async Task<IActionResult> VerifyResetOtpAsync([FromBody] VerifyResetOtpRequest request)
        {
            if (request == null)
            {
                return BadRequest("Verify reset OTP request cannot be null.");
            }
            var response = await _identityService.VerifyResetOtpAsync(request);
            if (response.IsSuccess)
            {
                return Ok(response.Value);
            }
            return ErrorResponse<string>.WithError(response);
        }
        [HttpPost("reset-password-with-otp")]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordWithOtpRequest request)
        {
            if (request == null)
            {
                return BadRequest("Reset password request cannot be null.");
            }
            var response = await _identityService.ResetPasswordAsync(request);
            if (response.IsSuccess)
            {
                return Ok(response.Value);
            }
            return ErrorResponse<string>.WithError(response);
        }
    }
}
