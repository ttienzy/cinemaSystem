using Application.Common.Interfaces.Services;
using Infrastructure.Hubs.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Infrastructure.Hubs
{
    [Authorize]
    public class SeatHub : Hub
    {
        private readonly ISeatLockService _seatLockService;
        private readonly ILogger<SeatHub> _logger;

        public SeatHub(ISeatLockService seatLockService, ILogger<SeatHub> logger)
        {
            _seatLockService = seatLockService;
            _logger = logger;
        }

        public async Task JoinShowtimeGroup(Guid showtimeId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, showtimeId.ToString());
            _logger.LogInformation("Client {ConnectionId} joined showtime group {ShowtimeId}", Context.ConnectionId, showtimeId);
        }

        public async Task LeaveShowtimeGroup(Guid showtimeId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, showtimeId.ToString());
            _logger.LogInformation("Client {ConnectionId} left showtime group {ShowtimeId}", Context.ConnectionId, showtimeId);
        }

        public async Task SelectSeat(Guid showtimeId, Guid seatId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return;

            await Clients.OthersInGroup(showtimeId.ToString()).SendAsync("SeatSelecting", new { SeatId = seatId, UserId = userId });
        }

        public async Task UnselectSeat(Guid showtimeId, Guid seatId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await Clients.OthersInGroup(showtimeId.ToString()).SendAsync("SeatUnselecting", new { SeatId = seatId, UserId = userId });
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Connected: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Disconnected: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
