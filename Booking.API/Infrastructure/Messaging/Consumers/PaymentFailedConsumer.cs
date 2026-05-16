using Cinema.Contracts.Events;
using Booking.API.Application.DTOs.Requests;
using Booking.API.Infrastructure.Persistence.Repositories;
using Cinema.Shared.Models;
using MassTransit;

namespace Booking.API.Infrastructure.Messaging.Consumers;

public class PaymentFailedConsumer : IConsumer<PaymentFailedEvent>
{
    private readonly IBookingService _bookingService;
    private readonly IBookingRepository _bookingRepository;
    private readonly ILogger<PaymentFailedConsumer> _logger;

    public PaymentFailedConsumer(
        IBookingService bookingService,
        IBookingRepository bookingRepository,
        ILogger<PaymentFailedConsumer> logger)
    {
        _bookingService = bookingService;
        _bookingRepository = bookingRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "MassTransit consumed PaymentFailedEvent for booking {BookingId}, reason: {Reason}",
            message.BookingId,
            message.Reason);

        try
        {
            var booking = await _bookingRepository.GetByIdAsync(message.BookingId);
            if (booking is null)
            {
                _logger.LogWarning(
                    "Booking {BookingId} not found when handling payment failure",
                    message.BookingId);
                return;
            }

            if (!booking.IsPending())
            {
                _logger.LogInformation(
                    "Skipping payment failure cancellation for booking {BookingId} because status is {Status}",
                    message.BookingId,
                    booking.Status);
                return;
            }

            // Cancel the booking and release seats
            var cancelRequest = new CancelBookingRequest
            {
                UserId = booking.UserId,
                CancellationReason = $"Payment failed: {message.Reason}"
            };

            var cancelResult = await _bookingService.CancelBookingAsync(
                message.BookingId,
                cancelRequest);

            if (cancelResult.Success)
            {
                _logger.LogInformation(
                    "Successfully cancelled booking {BookingId} after payment failure. Seats released.",
                    message.BookingId);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to cancel booking {BookingId} - booking may not exist or already processed",
                    message.BookingId);
            }
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex,
                "Cannot cancel booking {BookingId}: {Message}",
                message.BookingId,
                ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex,
                "Authorization issue cancelling booking {BookingId}: {Message}",
                message.BookingId,
                ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error handling PaymentFailedEvent for booking {BookingId}",
                message.BookingId);

            throw; // Trigger MassTransit retry
        }
    }
}
