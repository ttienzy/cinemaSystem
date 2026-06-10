using Booking.API.Client;

namespace Booking.API.Hubs.Interfaces;

public interface IAdminDashboardHubClient
{
    Task NewBooking(DashboardRecentActivityDto activity);
}
