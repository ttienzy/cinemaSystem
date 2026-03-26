namespace Application.Common.Interfaces.Security
{
    /// <summary>
    /// Advanced user management — list, lock/unlock accounts.
    /// Separated from IIdentityUserService to maintain Single Responsibility.
    /// </summary>
    public interface IUserManagementService
    {
        /// <summary>
        /// Get paginated user list, supports filtering by role/status/search keyword.
        /// </summary>
        Task<PagedUserResult> GetAllUsersAsync(
            string? role, string? search, bool? isLocked,
            int page, int pageSize);

        /// <summary>
        /// Lock account — user cannot login until unlocked.
        /// </summary>
        Task LockUserAsync(Guid userId);

        /// <summary>
        /// Unlock account — allows login again.
        /// </summary>
        Task UnlockUserAsync(Guid userId);
    }

    /// <summary>
    /// Paginated result of user list.
    /// </summary>
    public class PagedUserResult
    {
        public List<UserListItem> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    /// <summary>
    /// User information in list (summary, excludes sensitive data).
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
