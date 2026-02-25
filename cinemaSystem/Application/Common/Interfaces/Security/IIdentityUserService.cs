using Shared.Common.Base;
using Shared.Models.IdentityModels;
using Shared.Models.IdentityModels.Otps;

namespace Application.Common.Interfaces.Security
{
    /// <summary>
    /// Identity user management — registration, login, profile, password reset.
    /// </summary>
    public interface IIdentityUserService
    {
        Task<BaseResponse<LoginResponse>> LoginUserAsync(LoginRequest request);
        Task<BaseResponse<string>> RegisterUserAsync(RegisterRequest request);
        Task<BaseResponse<UserProfileResponse>> GetUserProfileAsync(Guid userId);
        Task<BaseResponse<string>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
        Task<BaseResponse<string>> ChangePasswordAsync(ChangePasswordRequest request);
        Task<BaseResponse<string>> ForgotPasswordAsync(string email);
        Task<BaseResponse<string>> VerifyResetOtpAsync(VerifyResetOtpRequest request);
        Task<BaseResponse<string>> ResetPasswordAsync(ResetPasswordWithOtpRequest request);
        Task<BaseResponse<string>> ResendOtpAsync(string email);
    }
}
