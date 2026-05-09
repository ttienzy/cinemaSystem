using Booking.API.Infrastructure.Hubs.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Booking.API.Infrastructure.Hubs;

[Authorize]
public class AdminDashboardHub : Hub<IAdminDashboardHubClient>
{
    private const string DashboardGroupName = "admin-dashboard";

    public Task JoinDashboard()
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, DashboardGroupName);
    }

    public Task LeaveDashboard()
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, DashboardGroupName);
    }

    public static string GetDashboardGroupName() => DashboardGroupName;
}
