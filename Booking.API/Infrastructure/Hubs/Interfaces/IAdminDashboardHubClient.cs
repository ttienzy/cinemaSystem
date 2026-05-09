using Booking.API.Application.DTOs.Responses;

namespace Booking.API.Infrastructure.Hubs.Interfaces;

public interface IAdminDashboardHubClient
{
    Task NewBooking(DashboardRecentActivityDto activity);
}
