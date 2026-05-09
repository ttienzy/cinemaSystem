using System.Net;
using System.Net.Mail;
using System.Text;
using Booking.API.Infrastructure.Configuration;
using Booking.API.Application.DTOs;
using Microsoft.Extensions.Options;

namespace Booking.API.Infrastructure.Notifications.Services;

public class EmailService : IEmailService
{
    private readonly SmtpOptions _smtpOptions;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<SmtpOptions> smtpOptions,
        ILogger<EmailService> logger)
    {
        _smtpOptions = smtpOptions.Value;
        _logger = logger;
    }

    public async Task<bool> SendBookingConfirmationAsync(BookingEmailDto booking)
    {
        try
        {
            var subject = $"Booking Confirmation - {booking.BookingCode}";
            var htmlBody = GenerateBookingConfirmationHtml(booking);
            var plainTextBody = GenerateBookingConfirmationText(booking);

            return await SendEmailAsync(booking.CustomerEmail, subject, htmlBody, plainTextBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send booking confirmation email for booking {BookingId}", booking.Id);
            return false;
        }
    }

    public async Task<bool> SendBookingCancellationAsync(BookingEmailDto booking, string reason)
    {
        try
        {
            var subject = $"Booking Cancelled - {booking.BookingCode}";
            var htmlBody = GenerateBookingCancellationHtml(booking, reason);
            var plainTextBody = GenerateBookingCancellationText(booking, reason);

            return await SendEmailAsync(booking.CustomerEmail, subject, htmlBody, plainTextBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send booking cancellation email for booking {BookingId}", booking.Id);
            return false;
        }
    }

    public async Task<bool> SendBookingReminderAsync(BookingEmailDto booking)
    {
        try
        {
            var subject = $"Showtime Reminder - {booking.BookingCode}";
            var htmlBody = GenerateBookingReminderHtml(booking);
            var plainTextBody = GenerateBookingReminderText(booking);

            return await SendEmailAsync(booking.CustomerEmail, subject, htmlBody, plainTextBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send booking reminder email for booking {BookingId}", booking.Id);
            return false;
        }
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody, string? plainTextBody = null)
    {
        try
        {
            using var client = new SmtpClient(_smtpOptions.Host, _smtpOptions.Port);
            client.EnableSsl = _smtpOptions.EnableSsl;
            client.Timeout = _smtpOptions.TimeoutSeconds * 1000;
            client.Credentials = new NetworkCredential(_smtpOptions.Username, _smtpOptions.Password);

            using var message = new MailMessage();
            message.From = new MailAddress(_smtpOptions.FromEmail, _smtpOptions.FromName);
            message.To.Add(toEmail);
            message.Subject = subject;
            message.IsBodyHtml = true;
            message.Body = htmlBody;

            // Add plain text alternative if provided
            if (!string.IsNullOrEmpty(plainTextBody))
            {
                var plainView = AlternateView.CreateAlternateViewFromString(plainTextBody, Encoding.UTF8, "text/plain");
                message.AlternateViews.Add(plainView);
            }

            await client.SendMailAsync(message);

            _logger.LogInformation("Email sent successfully to {Email} with subject: {Subject}", toEmail, subject);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email} with subject: {Subject}", toEmail, subject);
            return false;
        }
    }

    private string GenerateBookingConfirmationHtml(BookingEmailDto booking)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Booking Confirmation</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #2c3e50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .booking-details {{ background-color: white; padding: 15px; margin: 10px 0; border-radius: 5px; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
        .success {{ color: #27ae60; font-weight: bold; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎬 Booking Confirmed!</h1>
        </div>
        <div class='content'>
            <p>Dear {booking.CustomerName},</p>
            <p class='success'>Your booking has been confirmed successfully!</p>
            
            <div class='booking-details'>
                <h3>Booking Details</h3>
                <p><strong>Booking Code:</strong> {booking.BookingCode}</p>
                <p><strong>Movie:</strong> {booking.MovieTitle}</p>
                <p><strong>Cinema:</strong> {booking.CinemaName}</p>
                <p><strong>Hall:</strong> {booking.CinemaHallName}</p>
                <p><strong>Showtime:</strong> {booking.ShowtimeDate:dddd, MMMM dd, yyyy} at {booking.ShowtimeDate:HH:mm}</p>
                <p><strong>Seats:</strong> {string.Join(", ", booking.BookingSeats.Select(s => s.SeatNumber))}</p>
                <p><strong>Total Amount:</strong> {booking.TotalAmount:C}</p>
                <p><strong>Status:</strong> {booking.Status}</p>
            </div>
            
            <p><strong>Important:</strong> Please arrive at the cinema at least 15 minutes before showtime.</p>
            <p>Show this email or your booking code at the cinema for entry.</p>
        </div>
        <div class='footer'>
            <p>Thank you for choosing our cinema!</p>
            <p>This is an automated email. Please do not reply.</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GenerateBookingConfirmationText(BookingEmailDto booking)
    {
        return $@"
BOOKING CONFIRMED!

Dear {booking.CustomerName},

Your booking has been confirmed successfully!

Booking Details:
- Booking Code: {booking.BookingCode}
- Movie: {booking.MovieTitle}
- Cinema: {booking.CinemaName}
- Hall: {booking.CinemaHallName}
- Showtime: {booking.ShowtimeDate:dddd, MMMM dd, yyyy} at {booking.ShowtimeDate:HH:mm}
- Seats: {string.Join(", ", booking.BookingSeats.Select(s => s.SeatNumber))}
- Total Amount: {booking.TotalAmount:C}
- Status: {booking.Status}

Important: Please arrive at the cinema at least 15 minutes before showtime.
Show this email or your booking code at the cinema for entry.

Thank you for choosing our cinema!
This is an automated email. Please do not reply.
";
    }

    private string GenerateBookingCancellationHtml(BookingEmailDto booking, string reason)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Booking Cancelled</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #e74c3c; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .booking-details {{ background-color: white; padding: 15px; margin: 10px 0; border-radius: 5px; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
        .cancelled {{ color: #e74c3c; font-weight: bold; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🚫 Booking Cancelled</h1>
        </div>
        <div class='content'>
            <p>Dear {booking.CustomerName},</p>
            <p class='cancelled'>Your booking has been cancelled.</p>
            <p><strong>Reason:</strong> {reason}</p>
            
            <div class='booking-details'>
                <h3>Cancelled Booking Details</h3>
                <p><strong>Booking Code:</strong> {booking.BookingCode}</p>
                <p><strong>Movie:</strong> {booking.MovieTitle}</p>
                <p><strong>Showtime:</strong> {booking.ShowtimeDate:dddd, MMMM dd, yyyy} at {booking.ShowtimeDate:HH:mm}</p>
                <p><strong>Amount:</strong> {booking.TotalAmount:C}</p>
            </div>
            
            <p>If payment was processed, refund will be issued within 3-5 business days.</p>
        </div>
        <div class='footer'>
            <p>We apologize for any inconvenience.</p>
            <p>This is an automated email. Please do not reply.</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GenerateBookingCancellationText(BookingEmailDto booking, string reason)
    {
        return $@"
BOOKING CANCELLED

Dear {booking.CustomerName},

Your booking has been cancelled.
Reason: {reason}

Cancelled Booking Details:
- Booking Code: {booking.BookingCode}
- Movie: {booking.MovieTitle}
- Showtime: {booking.ShowtimeDate:dddd, MMMM dd, yyyy} at {booking.ShowtimeDate:HH:mm}
- Amount: {booking.TotalAmount:C}

If payment was processed, refund will be issued within 3-5 business days.

We apologize for any inconvenience.
This is an automated email. Please do not reply.
";
    }

    private string GenerateBookingReminderHtml(BookingEmailDto booking)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Showtime Reminder</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #f39c12; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .booking-details {{ background-color: white; padding: 15px; margin: 10px 0; border-radius: 5px; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
        .reminder {{ color: #f39c12; font-weight: bold; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>⏰ Showtime Reminder</h1>
        </div>
        <div class='content'>
            <p>Dear {booking.CustomerName},</p>
            <p class='reminder'>Your movie starts in 1 hour!</p>
            
            <div class='booking-details'>
                <h3>Your Booking</h3>
                <p><strong>Booking Code:</strong> {booking.BookingCode}</p>
                <p><strong>Movie:</strong> {booking.MovieTitle}</p>
                <p><strong>Cinema:</strong> {booking.CinemaName}</p>
                <p><strong>Hall:</strong> {booking.CinemaHallName}</p>
                <p><strong>Showtime:</strong> {booking.ShowtimeDate:dddd, MMMM dd, yyyy} at {booking.ShowtimeDate:HH:mm}</p>
                <p><strong>Seats:</strong> {string.Join(", ", booking.BookingSeats.Select(s => s.SeatNumber))}</p>
            </div>
            
            <p><strong>Don't forget:</strong> Arrive at least 15 minutes early!</p>
        </div>
        <div class='footer'>
            <p>Enjoy your movie!</p>
            <p>This is an automated email. Please do not reply.</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GenerateBookingReminderText(BookingEmailDto booking)
    {
        return $@"
SHOWTIME REMINDER

Dear {booking.CustomerName},

Your movie starts in 1 hour!

Your Booking:
- Booking Code: {booking.BookingCode}
- Movie: {booking.MovieTitle}
- Cinema: {booking.CinemaName}
- Hall: {booking.CinemaHallName}
- Showtime: {booking.ShowtimeDate:dddd, MMMM dd, yyyy} at {booking.ShowtimeDate:HH:mm}
- Seats: {string.Join(", ", booking.BookingSeats.Select(s => s.SeatNumber))}

Don't forget: Arrive at least 15 minutes early!

Enjoy your movie!
This is an automated email. Please do not reply.
";
    }
}

