namespace Application.Common.Interfaces.Security
{
    /// <summary>
    /// Quản lý users nâng cao — danh sách, khóa/mở khóa tài khoản.
    /// Tách khỏi IIdentityUserService để giữ Single Responsibility.
    /// </summary>
    public interface IUserManagementService
    {
        /// <summary>
        /// Lấy danh sách users phân trang, hỗ trợ lọc theo role/trạng thái/từ khóa tìm kiếm.
        /// </summary>
        Task<PagedUserResult> GetAllUsersAsync(
            string? role, string? search, bool? isLocked,
            int page, int pageSize);

        /// <summary>
        /// Khóa tài khoản — user không thể đăng nhập cho đến khi mở khóa.
        /// </summary>
        Task LockUserAsync(Guid userId);

        /// <summary>
        /// Mở khóa tài khoản — cho phép đăng nhập trở lại.
        /// </summary>
        Task UnlockUserAsync(Guid userId);
    }

    /// <summary>
    /// Kết quả phân trang danh sách users.
    /// </summary>
    public class PagedUserResult
    {
        public List<UserListItem> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    /// <summary>
    /// Thông tin user trong danh sách (tóm tắt, không chứa thông tin nhạy cảm).
    /// </summary>
    public class UserListItem
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public List<string> Roles { get; set; } = new();
        public bool IsLocked { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
