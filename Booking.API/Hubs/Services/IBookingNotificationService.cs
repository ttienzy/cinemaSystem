namespace Booking.API.Hubs.Services;

public interface IBookingNotificationService
{
    Task NotifyBookingConfirmedAsync(Guid bookingId, string status, CancellationToken cancellationToken = default);
    Task NotifyBookingFailedAsync(Guid bookingId, string reason, CancellationToken cancellationToken = default);
}
