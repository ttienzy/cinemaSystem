#if false // Disabled during Booking refactor: Redis/SignalR/RabbitMQ integration is paused.
using Booking.API.Hubs.Constants;

namespace Booking.API.Hubs.Builders;

public static class HubGroupNameBuilder
{
    public static string ForShowtime(Guid showtimeId) => $"showtime:{showtimeId}";

    public static string ForBooking(Guid bookingId) => $"booking-{bookingId}";

    public static string ForAdminDashboard() => HubConstants.DashboardGroupName;
}
#endif
