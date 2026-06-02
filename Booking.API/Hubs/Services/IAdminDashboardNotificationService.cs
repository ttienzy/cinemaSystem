#if false // Disabled during Booking refactor: Redis/SignalR/RabbitMQ integration is paused.
namespace Booking.API.Hubs.Services;

public interface IAdminDashboardNotificationService
{
    Task PublishBookingCompletedAsync(
        Guid bookingId,
        decimal amount,
        string customerName,
        DateTime occurredAtUtc,
        CancellationToken cancellationToken = default);
}
#endif
