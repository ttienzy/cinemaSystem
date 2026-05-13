using Booking.API.Infrastructure.Hubs.Constants;

namespace Booking.API.Infrastructure.Hubs.Builders;

public static class HubGroupNameBuilder
{
    public static string ForShowtime(Guid showtimeId) => $"showtime:{showtimeId}";

    public static string ForBooking(Guid bookingId) => $"booking-{bookingId}";

    public static string ForAdminDashboard() => HubConstants.DashboardGroupName;
}
