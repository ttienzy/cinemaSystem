namespace Shared.Models.IdentityModels
{
    public class CreateStaffRequest
    {
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // e.g., Manager, Staff
        public string? PhoneNumber { get; set; }
    }
}
