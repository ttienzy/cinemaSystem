using Application.Common.Interfaces.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Identity;

namespace Infrastructure.Identity.Services
{
    /// <summary>
    /// Implementation quản lý users — sử dụng ASP.NET Identity UserManager.
    /// Cung cấp chức năng danh sách users (phân trang), khóa/mở khóa tài khoản.
    /// </summary>
    public class UserManagementService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager) : IUserManagementService
    {
        /// <summary>
        /// Lấy danh sách users — hỗ trợ lọc theo role, trạng thái khóa, và từ khóa tìm kiếm.
        /// Tìm kiếm theo username, email hoặc fullname (PhoneNumber field).
        /// </summary>
        public async Task<PagedUserResult> GetAllUsersAsync(
            string? role, string? search, bool? isLocked,
            int page, int pageSize)
        {
            var query = userManager.Users.AsQueryable();

            // Lọc theo từ khóa tìm kiếm (username hoặc email)
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u =>
                    u.UserName!.Contains(search) ||
                    u.Email!.Contains(search));
            }

            // Lọc theo trạng thái khóa
            if (isLocked.HasValue)
            {
                if (isLocked.Value)
                    query = query.Where(u => u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow);
                else
                    query = query.Where(u => u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow);
            }

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderByDescending(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Map users sang UserListItem kèm roles
            var items = new List<UserListItem>();
            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);

                // Lọc theo role nếu có
                if (!string.IsNullOrEmpty(role) && !roles.Contains(role))
                    continue;

                items.Add(new UserListItem
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    PhoneNumber = user.PhoneNumber,
                    Roles = roles.ToList(),
                    IsLocked = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow
                });
            }

            return new PagedUserResult
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// Khóa tài khoản — đặt LockoutEnd = 100 năm sau (vĩnh viễn cho đến khi mở khóa).
        /// </summary>
        public async Task LockUserAsync(Guid userId)
        {
            var user = await userManager.FindByIdAsync(userId.ToString())
                ?? throw new KeyNotFoundException($"Không tìm thấy user với ID: {userId}");

            await userManager.SetLockoutEnabledAsync(user, true);
            await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
        }

        /// <summary>
        /// Mở khóa tài khoản — xóa LockoutEnd, cho phép đăng nhập lại.
        /// </summary>
        public async Task UnlockUserAsync(Guid userId)
        {
            var user = await userManager.FindByIdAsync(userId.ToString())
                ?? throw new KeyNotFoundException($"Không tìm thấy user với ID: {userId}");

            await userManager.SetLockoutEndDateAsync(user, null);
            await userManager.ResetAccessFailedCountAsync(user);
        }
    }
}
