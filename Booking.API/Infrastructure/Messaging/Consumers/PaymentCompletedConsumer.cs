using Cinema.Contracts.Events;
using Booking.API.Application.DTOs.Requests;
using Booking.API.Application.DTOs.Responses;
using Booking.API.Infrastructure.Hubs.Services;
using Cinema.Shared.Models;
using MassTransit;

namespace Booking.API.Infrastructure.Messaging.Consumers;

public class PaymentCompletedConsumer : IConsumer<PaymentCompletedEvent>
{
    private readonly IBookingService _bookingService;
    private readonly IEmailService _emailService;
    private readonly IAdminDashboardNotificationService _adminDashboardNotificationService;
    private readonly ILogger<PaymentCompletedConsumer> _logger;

    public PaymentCompletedConsumer(
        IBookingService bookingService,
        IEmailService emailService,
        IAdminDashboardNotificationService adminDashboardNotificationService,
        ILogger<PaymentCompletedConsumer> logger)
    {
        _bookingService = bookingService;
        _emailService = emailService;
        _adminDashboardNotificationService = adminDashboardNotificationService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
    {
        var message = context.Message;
        
        _logger.LogInformation(
            "MassTransit consumed PaymentCompletedEvent for booking {BookingId}, transaction {TransactionId}",
            message.BookingId,
            message.TransactionId);

        try
        {
            // Step 1: Confirm the booking (change status from Pending to Confirmed)
            var result = await _bookingService.ConfirmBookingAsync(
                message.BookingId,
                message.TransactionId);

            if (result.Success)
            {
                _logger.LogInformation(
                    "Successfully confirmed booking {BookingId} after payment completion",
                    message.BookingId);

                await SendConfirmationEmailAsync(message);
                await PublishDashboardActivityAsync(message);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to confirm booking {BookingId}: {Message}",
                    message.BookingId,
                    result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error handling PaymentCompletedEvent for booking {BookingId}",
                message.BookingId);

            throw; // Re-throw to trigger MassTransit retry
        }
    }

    private async Task SendConfirmationEmailAsync(PaymentCompletedEvent message)
    {
        try
        {
            var bookingResult = await _bookingService.GetBookingByIdAsync(message.BookingId);
            if (!bookingResult.Success || bookingResult.Data is null)
            {
                _logger.LogWarning(
                    "Could not fetch booking details for email. Booking {BookingId}",
                    message.BookingId);
                return;
            }

            var emailSent = await _emailService.SendBookingConfirmationAsync(
                BuildBookingConfirmationEmail(bookingResult.Data, message));

            if (emailSent)
            {
                _logger.LogInformation(
                    "Confirmation email sent successfully for booking {BookingId} to {Email}",
                    message.BookingId,
                    message.CustomerEmail);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to send confirmation email for booking {BookingId} to {Email}",
                    message.BookingId,
                    message.CustomerEmail);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error sending email for booking {BookingId}",
                message.BookingId);
        }
    }

    private async Task PublishDashboardActivityAsync(PaymentCompletedEvent message)
    {
        try
        {
            await _adminDashboardNotificationService.PublishBookingCompletedAsync(
                message.BookingId,
                message.Amount,
                message.CustomerName,
                message.CompletedAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error publishing dashboard activity for booking {BookingId}",
                message.BookingId);
        }
    }

    private static BookingEmailDto BuildBookingConfirmationEmail(
        BookingResponse booking,
        PaymentCompletedEvent message)
    {
        return new BookingEmailDto
        {
            Id = booking.BookingId,
            BookingCode = booking.BookingId.ToString("N")[..8].ToUpperInvariant(),
            CustomerEmail = message.CustomerEmail,
            CustomerName = message.CustomerName,
            MovieTitle = booking.ShowtimeDetails?.MovieTitle ?? "Unknown Movie",
            CinemaName = booking.ShowtimeDetails?.CinemaName ?? "Unknown Cinema",
            CinemaHallName = booking.ShowtimeDetails?.CinemaHallName ?? "Unknown Hall",
            ShowtimeDate = booking.ShowtimeDetails?.StartTime ?? DateTime.UtcNow,
            TotalAmount = message.Amount,
            Status = booking.Status,
            BookingSeats = booking.Seats.Select(seat => new Application.DTOs.BookingSeatDto
            {
                SeatNumber = $"{seat.Row}{seat.Number}",
                SeatPrice = seat.Price
            }).ToList()
        };
    }
}
