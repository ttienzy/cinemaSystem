using Booking.API.Application.DTOs.Requests;
using Cinema.EventBus.Abstractions;
using Cinema.EventBus.Events;
using Cinema.Shared.Models;

namespace Booking.API.Infrastructure.Messaging.EventHandlers;

/// <summary>
/// Handles PaymentFailedIntegrationEvent from Payment service
/// </summary>
public class PaymentFailedIntegrationEventHandler
    : IIntegrationEventHandler<PaymentFailedIntegrationEvent>
{
    private readonly IBookingService _bookingService;
    private readonly IBookingRepository _bookingRepository;
    private readonly ILogger<PaymentFailedIntegrationEventHandler> _logger;

    public PaymentFailedIntegrationEventHandler(
        IBookingService bookingService,
        IBookingRepository bookingRepository,
        ILogger<PaymentFailedIntegrationEventHandler> logger)
    {
        _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
        _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(PaymentFailedIntegrationEvent @event)
    {
        _logger.LogInformation(
            "Handling PaymentFailedIntegrationEvent for booking {BookingId}, reason: {Reason}",
            @event.BookingId,
            @event.Reason);

        try
        {
            var booking = await _bookingRepository.GetByIdAsync(@event.BookingId);
            if (booking is null)
            {
                _logger.LogWarning(
                    "Booking {BookingId} not found when handling payment failure",
                    @event.BookingId);
                return;
            }

            if (!booking.IsPending())
            {
                _logger.LogInformation(
                    "Skipping payment failure cancellation for booking {BookingId} because status is {Status}",
                    @event.BookingId,
                    booking.Status);
                return;
            }

            // Cancel the booking and release seats
            var cancelRequest = new CancelBookingRequest
            {
                UserId = booking.UserId,
                CancellationReason = $"Payment failed: {@event.Reason}"
            };

            var cancelResult = await _bookingService.CancelBookingAsync(
                @event.BookingId,
                cancelRequest);

            if (cancelResult.Success)
            {
                _logger.LogInformation(
                    "Successfully cancelled booking {BookingId} after payment failure. Seats released.",
                    @event.BookingId);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to cancel booking {BookingId} - booking may not exist or already processed",
                    @event.BookingId);
            }
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex,
                "Cannot cancel booking {BookingId}: {Message}",
                @event.BookingId,
                ex.Message);

            // Don't throw - this is expected for invalid state transitions
            // The booking might have already been cancelled or expired
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex,
                "Authorization issue cancelling booking {BookingId}: {Message}",
                @event.BookingId,
                ex.Message);

            // Don't throw - log and continue
            // This shouldn't happen in event handler context, but handle gracefully
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error handling PaymentFailedIntegrationEvent for booking {BookingId}",
                @event.BookingId);

            // Re-throw to trigger retry mechanism (if configured)
            throw;
        }
    }
}


