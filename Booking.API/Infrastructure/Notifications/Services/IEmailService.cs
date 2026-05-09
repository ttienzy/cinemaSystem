using Booking.API.Application.DTOs;

namespace Booking.API.Infrastructure.Notifications.Services;

public interface IEmailService
{
    /// <summary>
    /// Send booking confirmation email to customer
    /// </summary>
    Task<bool> SendBookingConfirmationAsync(BookingEmailDto booking);

    /// <summary>
    /// Send booking cancellation email to customer
    /// </summary>
    Task<bool> SendBookingCancellationAsync(BookingEmailDto booking, string reason);

    /// <summary>
    /// Send booking reminder email (1 hour before showtime)
    /// </summary>
    Task<bool> SendBookingReminderAsync(BookingEmailDto booking);

    /// <summary>
    /// Send generic email
    /// </summary>
    Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody, string? plainTextBody = null);
}

