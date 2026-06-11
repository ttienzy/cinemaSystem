using Booking.API.Hubs;
using Booking.API.Hubs.Builders;
using Booking.API.Hubs.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Booking.API.Hubs.Services;

public class BookingNotificationService : IBookingNotificationService
{
    private readonly IHubContext<BookingHub, IBookingHubClient> _hubContext;
    private readonly ILogger<BookingNotificationService> _logger;

    public BookingNotificationService(
        IHubContext<BookingHub, IBookingHubClient> hubContext,
        ILogger<BookingNotificationService> logger)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task NotifyBookingConfirmedAsync(
        Guid bookingId,
        string status,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _hubContext.Clients
                .Group(HubGroupNameBuilder.ForBooking(bookingId))
                .BookingConfirmed(bookingId, status);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to broadcast booking confirmed notification for {BookingId}", bookingId);
        }
    }

    public async Task NotifyBookingFailedAsync(
        Guid bookingId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _hubContext.Clients
                .Group(HubGroupNameBuilder.ForBooking(bookingId))
                .BookingFailed(bookingId, reason);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to broadcast booking failure notification for {BookingId}", bookingId);
        }
    }
}
