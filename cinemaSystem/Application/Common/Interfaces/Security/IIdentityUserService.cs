using Shared.Models.IdentityModels;
using Shared.Models.IdentityModels.Otps;

namespace Application.Common.Interfaces.Security
{
    /// <summary>
    /// Identity user management — registration, login, profile, password reset.
    /// </summary>
    public interface IIdentityUserService
    {
        Task<LoginResponse> LoginUserAsync(LoginRequest request);
        Task RegisterUserAsync(RegisterRequest request);
        Task<UserProfileResponse> GetUserProfileAsync(Guid userId);
        Task UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
        Task ChangePasswordAsync(ChangePasswordRequest request);
        Task ForgotPasswordAsync(string email);
        Task<bool> VerifyResetOtpAsync(VerifyResetOtpRequest request);
        Task ResetPasswordAsync(ResetPasswordWithOtpRequest request);
        Task ResendOtpAsync(string email);
        Task CreateStaffAsync(CreateStaffRequest request);
        Task UpdateUserRoleAsync(Guid userId, UpdateUserRoleRequest request);
    }
}
