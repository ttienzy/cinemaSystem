using Application.Interfaces.Integrations;
using Application.Interfaces.Security;
using Infrastructure.Identity.Constants;
using Shared.Models.IdentityModels.Otps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Identity.Security
{
    public class OtpService : IOtpService
    {
        private readonly ICacheService _cacheService;
        private readonly Random _random = new Random();
        public OtpService(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }
        public async Task<string> GenerateOtpAsync(string userId, string email)
        {
            var otpCode = _random.Next(100000, 999999).ToString();
            var otpData = new PasswordResetOtp
            {
                UserId = userId,
                Email = email,
                OtpCode = otpCode,
            };
            await _cacheService.SetAsync($"password_reset_otp_{email.ToLower()}", otpData, TimeSpan.FromMinutes(OtpConstants.OtpExpirationMinutes));
            return otpCode;
        }


        public async Task<bool> ValidateOtpAsync(string email, string otpCode)
        {
            var cacheKey = $"password_reset_otp_{email.ToLower()}";
            var statusOtp = await _cacheService.ExistsAsync(cacheKey);
            if (!statusOtp)
            {
                return false; // OTP không tồn tại
            }
            var otpData = await _cacheService.GetAsync<PasswordResetOtp>(cacheKey);
            if (otpData == null || otpData.IsUsed || otpData.ExpiresAt < DateTime.UtcNow)
            {
                return false;
            }
            if (otpData.AttemptCount >= 3)
            {
                await _cacheService.RemoveAsync(cacheKey);
                return false;
            }
            if (otpData.OtpCode == otpCode)
            {
                otpData.IsUsed = true;
                await _cacheService.SetAsync(cacheKey, otpData, TimeSpan.FromMinutes(OtpConstants.OtpExpirationMinutes));
                return true; // OTP hợp lệ
            }
            else
            {
                otpData.AttemptCount++;
                await _cacheService.SetAsync(cacheKey, otpData);
                return false; // OTP không hợp lệ
            }
        }
    }
}
