#if false // Disabled during Booking refactor: Redis/SignalR/RabbitMQ integration is paused.
using Booking.API.Client;

namespace Booking.API.Hubs.Interfaces;

public interface IAdminDashboardHubClient
{
    Task NewBooking(DashboardRecentActivityDto activity);
}
#endif
