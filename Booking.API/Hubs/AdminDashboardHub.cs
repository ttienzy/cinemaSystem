using Booking.API.Hubs.Builders;
using Booking.API.Hubs.Interfaces;
using Cinema.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Booking.API.Hubs;

[Authorize(Roles = AppConstants.Roles.Admin)]
public class AdminDashboardHub : Hub<IAdminDashboardHubClient>
{
    public Task JoinDashboard()
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, HubGroupNameBuilder.ForAdminDashboard());
    }

    public Task LeaveDashboard()
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, HubGroupNameBuilder.ForAdminDashboard());
    }
}
