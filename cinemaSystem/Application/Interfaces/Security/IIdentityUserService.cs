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
        public Task<BaseResponse<string>> RegisterUserAsync(RegisterRequest request);  
        public Task<BaseResponse<LoginResponse>> LoginUserAsync(LoginRequest request);

        // User Profile Operations
        public Task<BaseResponse<UserProfileResponse>> GetUserProfileAsync(Guid userId);
        public Task<BaseResponse<string>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);

        // Password Reset Operations - OTP
        public Task<BaseResponse<string>> ChangePasswordAsync(ChangePasswordRequest request);
        public Task<BaseResponse<string>> ForgotPasswordAsync(string email);
        public Task<BaseResponse<string>> VerifyResetOtpAsync(VerifyResetOtpRequest request);
        public Task<BaseResponse<string>> ResetPasswordAsync(ResetPasswordWithOtpRequest request);
        public Task<BaseResponse<string>> ResendOtpAsync(string email);
    }
}
