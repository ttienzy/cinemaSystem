using Application.Common.Interfaces.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.IdentityModels;

namespace Api.Controllers
{
    /// <summary>
    /// Quản lý người dùng hệ thống — Chỉ dành cho Admin.
    /// Bao gồm: Xem danh sách users, chi tiết user, khóa/mở khóa tài khoản,
    /// tạo tài khoản staff, cập nhật vai trò.
    /// </summary>
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "Admin")]
    public class AdminUsersController(
        IIdentityUserService identityService,
        IUserManagementService userManagementService) : ControllerBase
    {
        /// <summary>
        /// Lấy danh sách tất cả users (phân trang, lọc theo role/trạng thái/từ khóa).
        /// Dùng cho trang quản lý users của Admin.
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
        /// Xem chi tiết thông tin 1 user (profile + role + trạng thái).
        /// </summary>
        [HttpGet("{userId:guid}")]
        public async Task<ActionResult<UserProfileResponse>> GetUserById(Guid userId)
        {
            return Ok(await identityService.GetUserProfileAsync(userId));
        }

        /// <summary>
        /// Khóa tài khoản user — ngăn không cho đăng nhập.
        /// Dùng khi phát hiện hành vi bất thường hoặc vi phạm chính sách.
        /// </summary>
        [HttpPut("{userId:guid}/lock")]
        public async Task<IActionResult> LockUser(Guid userId)
        {
            await userManagementService.LockUserAsync(userId);
            return Ok(new { message = "Tài khoản đã bị khóa." });
        }

        /// <summary>
        /// Mở khóa tài khoản user — cho phép đăng nhập trở lại.
        /// </summary>
        [HttpPut("{userId:guid}/unlock")]
        public async Task<IActionResult> UnlockUser(Guid userId)
        {
            await userManagementService.UnlockUserAsync(userId);
            return Ok(new { message = "Tài khoản đã được mở khóa." });
        }

        /// <summary>
        /// Tạo tài khoản nhân viên mới — tự động gửi email chào mừng.
        /// (Chuyển từ IdentityController sang đây cho đúng phân quyền Admin.)
        /// </summary>
        [HttpPost("staff")]
        public async Task<IActionResult> CreateStaff([FromBody] CreateStaffRequest request)
        {
            await identityService.CreateStaffAsync(request);
            return Ok(new { message = "Tạo tài khoản nhân viên thành công." });
        }

        /// <summary>
        /// Cập nhật vai trò (role) của user.
        /// Ví dụ: nâng Staff lên Manager, hoặc hạ Manager xuống Staff.
        /// </summary>
        [HttpPut("{userId:guid}/role")]
        public async Task<IActionResult> UpdateUserRole(Guid userId, [FromBody] UpdateUserRoleRequest request)
        {
            await identityService.UpdateUserRoleAsync(userId, request);
            return Ok(new { message = "Cập nhật vai trò thành công." });
        }
    }
}
