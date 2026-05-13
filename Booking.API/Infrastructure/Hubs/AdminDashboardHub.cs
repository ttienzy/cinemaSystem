using Booking.API.Infrastructure.Hubs.Builders;
using Booking.API.Infrastructure.Hubs.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Booking.API.Infrastructure.Hubs;

[Authorize]
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
