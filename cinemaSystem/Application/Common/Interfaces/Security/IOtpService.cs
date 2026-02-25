namespace Application.Common.Interfaces.Security
{
    /// <summary>
    /// OTP generation and validation for password reset flow.
    /// </summary>
    public interface IOtpService
    {
        Task<string> GenerateOtpAsync(string userId, string email);
        Task<bool> ValidateOtpAsync(string email, string otpCode);
    }
}
