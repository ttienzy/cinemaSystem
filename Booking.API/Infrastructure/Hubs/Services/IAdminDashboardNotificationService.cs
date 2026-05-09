namespace Booking.API.Infrastructure.Hubs.Services;

public interface IAdminDashboardNotificationService
{
    Task PublishBookingCompletedAsync(
        Guid bookingId,
        decimal amount,
        string customerName,
        DateTime occurredAtUtc,
        CancellationToken cancellationToken = default);
}
