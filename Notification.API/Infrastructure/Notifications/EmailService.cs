using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Options;
using Notification.API.Infrastructure.Configuration;

namespace Notification.API.Infrastructure.Notifications;

public class EmailService : IEmailService
{
    private static readonly CultureInfo CurrencyCulture = CultureInfo.GetCultureInfo("vi-VN");
    private static readonly TimeZoneInfo DisplayTimeZone = ResolveDisplayTimeZone();

    private readonly SmtpOptions _smtpOptions;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<SmtpOptions> smtpOptions,
        ILogger<EmailService> logger)
    {
        _smtpOptions = smtpOptions.Value;
        _logger = logger;
    }

    public async Task SendPaymentCompletedAsync(PaymentCompletedEmail email, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email.CustomerEmail))
        {
            throw new InvalidOperationException($"PaymentCompletedEvent for booking {email.BookingId} has no customer email.");
        }

        var subject = $"Booking Confirmation - {BuildBookingCode(email.BookingId)}";
        var htmlBody = BuildPaymentCompletedHtml(email);
        var plainTextBody = BuildPaymentCompletedText(email);

        await SendEmailAsync(email.CustomerEmail, subject, htmlBody, plainTextBody, email.BookingId, cancellationToken);
    }

    private async Task SendEmailAsync(
        string toEmail,
        string subject,
        string htmlBody,
        string plainTextBody,
        Guid bookingId,
        CancellationToken cancellationToken)
    {
        using var client = new SmtpClient(_smtpOptions.Host, _smtpOptions.Port)
        {
            EnableSsl = _smtpOptions.EnableSsl,
            Timeout = _smtpOptions.TimeoutSeconds * 1000,
            Credentials = new NetworkCredential(_smtpOptions.Username, _smtpOptions.Password)
        };

        using var message = new MailMessage
        {
            From = new MailAddress(_smtpOptions.FromEmail, _smtpOptions.FromName),
            Subject = subject,
            SubjectEncoding = Encoding.UTF8,
            BodyEncoding = Encoding.UTF8,
            HeadersEncoding = Encoding.UTF8,
            IsBodyHtml = true,
            Body = htmlBody
        };

        message.To.Add(toEmail);
        message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(
            plainTextBody,
            Encoding.UTF8,
            "text/plain"));

        await client.SendMailAsync(message, cancellationToken);

        _logger.LogInformation(
            "Payment completion email sent to {Email} for booking {BookingId}",
            toEmail,
            bookingId);
    }

    private static string BuildPaymentCompletedHtml(PaymentCompletedEmail email)
    {
        var bookingCode = BuildBookingCode(email.BookingId);

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Booking Confirmation</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #1f2937;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 24px;'>
        <h1 style='color: #0f766e;'>Booking confirmed</h1>
        <p>Hello {WebUtility.HtmlEncode(email.CustomerName)},</p>
        <p>Your payment was completed and your booking is confirmed.</p>
        <div style='background: #f8fafc; border: 1px solid #e2e8f0; padding: 16px; border-radius: 6px;'>
            <p><strong>Booking code:</strong> {bookingCode}</p>
            <p><strong>Booking ID:</strong> {email.BookingId}</p>
            <p><strong>Payment ID:</strong> {email.PaymentId}</p>
            <p><strong>Transaction ID:</strong> {WebUtility.HtmlEncode(email.TransactionId)}</p>
            <p><strong>Amount:</strong> {FormatCurrency(email.Amount)}</p>
            <p><strong>Completed at:</strong> {FormatDisplayTime(email.CompletedAt)}</p>
        </div>
        <p>Please arrive at the cinema at least 15 minutes before showtime.</p>
        <p>Thank you for choosing CinemaSystem.</p>
    </div>
</body>
</html>";
    }

    private static string BuildPaymentCompletedText(PaymentCompletedEmail email)
    {
        return $@"
BOOKING CONFIRMED

Hello {email.CustomerName},

Your payment was completed and your booking is confirmed.

Booking code: {BuildBookingCode(email.BookingId)}
Booking ID: {email.BookingId}
Payment ID: {email.PaymentId}
Transaction ID: {email.TransactionId}
Amount: {FormatCurrency(email.Amount)}
Completed at: {FormatDisplayTime(email.CompletedAt)}

Please arrive at the cinema at least 15 minutes before showtime.
Thank you for choosing CinemaSystem.
";
    }

    private static string BuildBookingCode(Guid bookingId) =>
        bookingId.ToString("N")[..8].ToUpperInvariant();

    private static string FormatCurrency(decimal amount) =>
        $"{amount.ToString("N0", CurrencyCulture)} VND";

    private static string FormatDisplayTime(DateTime utcDateTime)
    {
        var normalizedUtc = utcDateTime.Kind == DateTimeKind.Utc
            ? utcDateTime
            : DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

        return $"{TimeZoneInfo.ConvertTimeFromUtc(normalizedUtc, DisplayTimeZone):dd/MM/yyyy HH:mm} (GMT+7)";
    }

    private static TimeZoneInfo ResolveDisplayTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
        }
    }
}
