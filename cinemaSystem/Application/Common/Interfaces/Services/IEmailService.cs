using Shared.Models;
using Shared.Models.ExtenalModels;

namespace Application.Common.Interfaces.Services
{
    /// <summary>
    /// Email sending service — booking confirmations and general emails.
    /// </summary>
    public interface IEmailService
    {
        Task SendBookingConfirmationEmailAsync(string toEmail, EmailConfirmBookingResponse bookingInfo);
        Task SendEmailAsync(EmailRequest request);
    }
}
