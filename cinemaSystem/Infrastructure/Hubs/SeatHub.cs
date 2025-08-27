using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Hubs
{
    public class SeatHub : Hub
    {
        public async Task JoinEventGroup(Guid eventId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, eventId.ToString());
        }
        public async Task LeaveEventGroup(Guid eventId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, eventId.ToString());
        }
        public override async Task OnConnectedAsync()
        {
            // Optionally, you can log the connection or perform other actions here
            Console.WriteLine($"Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Optionally, you can log the disconnection or perform other actions here
            Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
