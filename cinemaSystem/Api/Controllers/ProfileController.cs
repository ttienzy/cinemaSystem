using Application.Common.Interfaces.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.IdentityModels;
using System.Security.Claims;

namespace Api.Controllers
{
    /// <summary>
    /// Hồ sơ cá nhân — Dành cho Customer đã đăng nhập.
    /// Tách từ IdentityController để giữ Single Responsibility.
    /// Tự động lấy userId từ JWT token, không cần truyền trong URL.
    /// </summary>
    [ApiController]
    [Route("api/profile")]
    [Authorize]
    public class ProfileController(IIdentityUserService identityService) : ControllerBase
    {
        /// <summary>
        /// Xem hồ sơ cá nhân — lấy userId từ token.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<UserProfileResponse>> GetMyProfile()
        {
            var userId = GetCurrentUserId();
            return Ok(await identityService.GetUserProfileAsync(userId));
        }

        /// <summary>
        /// Cập nhật hồ sơ cá nhân — chỉ sửa thông tin của chính mình.
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = GetCurrentUserId();
            await identityService.UpdateProfileAsync(userId, request);
            return Ok(new { message = "Cập nhật hồ sơ thành công." });
        }

        /// <summary>
        /// Đổi mật khẩu — yêu cầu mật khẩu cũ + mật khẩu mới.
        /// </summary>
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            await identityService.ChangePasswordAsync(request);
            return Ok(new { message = "Đổi mật khẩu thành công." });
        }

        private Guid GetCurrentUserId()
            => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
