using Shared.Common.Base;
using Shared.Models.IdentityModels;
using Shared.Models.IdentityModels.Otps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Security
{
    public interface IIdentityUserService
    {
        // User Registration, Login, Logout Operations
        Task<BaseResponse<string>> RegisterUserAsync(RegisterRequest request);  
        Task<BaseResponse<LoginResponse>> LoginUserAsync(LoginRequest request);

        // User Profile Operations
        Task<BaseResponse<UserProfileResponse>> GetUserProfileAsync(Guid userId);
        Task<BaseResponse<string>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);

        // Password Reset Operations - OTP
        Task<BaseResponse<string>> ChangePasswordAsync(ChangePasswordRequest request);
        Task<BaseResponse<string>> ForgotPasswordAsync(string email);
        Task<BaseResponse<string>> VerifyResetOtpAsync(VerifyResetOtpRequest request);
        Task<BaseResponse<string>> ResetPasswordAsync(ResetPasswordWithOtpRequest request);
        Task<BaseResponse<string>> ResendOtpAsync(string email);
    }
}
