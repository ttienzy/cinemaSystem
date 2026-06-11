using System.Globalization;
using System.Net;
using System.Text;
using Booking.API.Client;
using Booking.API.Repositories;
using Booking.API.Services;
using Cinema.Contracts.Events;

namespace Booking.API.Notifications.Email;

public class BookingConfirmationEmailService : IBookingConfirmationEmailService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IBookingResponseFactory _bookingResponseFactory;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<BookingConfirmationEmailService> _logger;

    public BookingConfirmationEmailService(
        IBookingRepository bookingRepository,
        IBookingResponseFactory bookingResponseFactory,
        IEmailSender emailSender,
        ILogger<BookingConfirmationEmailService> logger)
    {
        _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
        _bookingResponseFactory = bookingResponseFactory ?? throw new ArgumentNullException(nameof(bookingResponseFactory));
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SendPaymentCompletedEmailAsync(
        PaymentCompletedEvent message,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(message.CustomerEmail))
        {
            _logger.LogWarning(
                "Skipping booking confirmation email for booking {BookingId}: customer email is empty",
                message.BookingId);
            return;
        }

        var booking = await _bookingRepository.GetByIdWithSeatsAsync(message.BookingId);
        if (booking is null)
        {
            _logger.LogWarning(
                "Skipping booking confirmation email: booking {BookingId} was not found",
                message.BookingId);
            return;
        }

        var response = await _bookingResponseFactory.CreateAsync(booking);
        var subject = BuildSubject(response);
        var body = BuildBody(response, message);

        await _emailSender.SendAsync(
            new EmailMessage
            {
                To = message.CustomerEmail,
                Subject = subject,
                Body = body,
                IsHtml = true
            },
            cancellationToken);
    }

    private static string BuildSubject(BookingResponse booking)
    {
        var movieTitle = booking.ShowtimeDetails?.MovieTitle;
        return string.IsNullOrWhiteSpace(movieTitle)
            ? $"Booking confirmed - {booking.BookingId}"
            : $"Booking confirmed - {movieTitle}";
    }

    private static string BuildBody(BookingResponse booking, PaymentCompletedEvent payment)
    {
        var details = booking.ShowtimeDetails;
        var seats = booking.Seats.Count == 0
            ? "Updating"
            : string.Join(", ", booking.Seats.Select(seat => $"{seat.Row}{seat.Number}"));

        var rows = new Dictionary<string, string>
        {
            ["Booking ID"] = booking.BookingId.ToString(),
            ["Payment ID"] = payment.PaymentId.ToString(),
            ["Transaction"] = string.IsNullOrWhiteSpace(payment.TransactionId) ? "N/A" : payment.TransactionId,
            ["Customer"] = string.IsNullOrWhiteSpace(payment.CustomerName) ? "Customer" : payment.CustomerName,
            ["Movie"] = details?.MovieTitle ?? "Updating",
            ["Cinema"] = details?.CinemaName ?? "Updating",
            ["Hall"] = details?.CinemaHallName ?? "Updating",
            ["Showtime"] = details is null
                ? "Updating"
                : details.StartTime.ToLocalTime().ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
            ["Seats"] = seats,
            ["Amount"] = $"{booking.TotalPrice:N0} VND",
            ["Paid at"] = payment.CompletedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)
        };

        var builder = new StringBuilder();
        builder.AppendLine("<!doctype html>");
        builder.AppendLine("<html><body style=\"font-family:Arial,sans-serif;color:#111827;line-height:1.5\">");
        builder.AppendLine("<h2>Your cinema booking is confirmed</h2>");
        builder.AppendLine("<p>Thank you for your payment. Please keep this email for check-in.</p>");
        builder.AppendLine("<table cellpadding=\"8\" cellspacing=\"0\" style=\"border-collapse:collapse;border:1px solid #e5e7eb\">");

        foreach (var row in rows)
        {
            builder.Append("<tr>");
            builder.Append("<td style=\"border:1px solid #e5e7eb;background:#f9fafb;font-weight:600\">");
            builder.Append(WebUtility.HtmlEncode(row.Key));
            builder.Append("</td>");
            builder.Append("<td style=\"border:1px solid #e5e7eb\">");
            builder.Append(WebUtility.HtmlEncode(row.Value));
            builder.Append("</td>");
            builder.AppendLine("</tr>");
        }

        builder.AppendLine("</table>");
        builder.AppendLine("<p style=\"color:#6b7280\">This is an automated email from Cinema System.</p>");
        builder.AppendLine("</body></html>");

        return builder.ToString();
    }
}
