using Application.Interfaces.Integrations;
using Application.Interfaces.Persistences;
using Infrastructure.Hubs.Constants;
using Infrastructure.Redis.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Shared.Models.DataModels.ShowtimeDtos;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace Infrastructure.Hubs
{
    public class SeatHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> _connections = new();
        private readonly ICacheService _cacheService;
        private readonly IShowtimeService _showtimeService;
        private readonly ILogger<SeatHub> _logger;

        public SeatHub(ICacheService cacheService, IShowtimeService showtimeService, ILogger<SeatHub> logger)
        {
            _cacheService = cacheService;
            _showtimeService = showtimeService;
            _logger = logger;
        }

        public async Task JoinShowtimeGroup(Guid showtimeId)
        {
            var connectionId = Context.ConnectionId;
            LogUserInfo("JoinShowtimeGroup");

            await Groups.AddToGroupAsync(connectionId, showtimeId.ToString());
            await Clients.Group(showtimeId.ToString()).SendAsync(SignalMethodConstants.JoinShowtimeGroup, $"A client join room: {showtimeId}");
        }

        public async Task LeaveShowtimeGroup(Guid showtimeId)
        {
            var connectionId = Context.ConnectionId;
            LogUserInfo("LeaveShowtimeGroup");

            await Groups.RemoveFromGroupAsync(connectionId, showtimeId.ToString());
            await Clients.Group(showtimeId.ToString()).SendAsync(SignalMethodConstants.LeaveShowtimeGroup, $"A client left room: {showtimeId}");
        }

        public async Task SeatsReserved(Guid showtimeId, List<Guid> seatIds)
        {
            try
            {
                LogUserInfo("SeatsReserved");

                var results = await _cacheService.GetAsync<ShowtimeSeatingPlanResponse>(CacheKey.SeatingPlan(showtimeId));
                if (results == null)
                {
                    await Clients.Caller.SendAsync("SeatsReservedError", "Showtime not found.");
                    return;
                }

                results.Seats.ForEach(s =>
                {
                    if (seatIds.Contains(s.Id))
                    {
                        s.Status = Domain.Entities.CinemaAggreagte.Enum.SeatStatus.Reserved;
                        s.LastUpdated = DateTime.UtcNow;
                    }
                });

                await _cacheService.UpdateAsync(CacheKey.SeatingPlan(showtimeId), results);

                await Clients
                    .Group(showtimeId.ToString())
                    .SendAsync(SignalMethodConstants.OnSeatsReserved, seatIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SeatsReserved] Error reserving seats for showtime {ShowtimeId}", showtimeId);
                await Clients.Caller.SendAsync("SeatsReservedError", "An error occurred while reserving seats.");
            }
        }

        public async Task SeatsReleased(Guid showtimeId, List<Guid> seatIds)
        {
            try
            {
                var results = await _cacheService.GetAsync<ShowtimeSeatingPlanResponse>(CacheKey.SeatingPlan(showtimeId));
                if (results == null)
                    return;

                results.Seats.ForEach(s =>
                {
                    if (seatIds.Contains(s.Id))
                    {
                        s.Status = Domain.Entities.CinemaAggreagte.Enum.SeatStatus.Available;
                        s.LastUpdated = DateTime.UtcNow;
                    }
                });

                await _cacheService.UpdateAsync<ShowtimeSeatingPlanResponse>(CacheKey.SeatingPlan(showtimeId), results);
                await Clients.Group(showtimeId.ToString()).SendAsync(SignalMethodConstants.OnSeatsReleased, seatIds);

                var userId = GetUserId();
                _logger.LogInformation("User {UserId} released seats: {SeatIds}", userId ?? "Anonymous", string.Join(", ", seatIds));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SeatsReleased] Error releasing seats for showtime {ShowtimeId}", showtimeId);
                await Clients.Caller.SendAsync("SeatsReleasedError", "An error occurred while releasing seats.");
            }
        }

        public override async Task OnConnectedAsync()
        {
            LogUserInfo("OnConnectedAsync");

            var userId = GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                _logger.LogInformation("Authenticated user connected: {UserId} with ConnectionId: {ConnectionId}", userId, Context.ConnectionId);
                _connections.AddOrUpdate(userId, Context.ConnectionId, (_, _) => Context.ConnectionId);
            }
            else
            {
                _logger.LogInformation("Anonymous user connected: {ConnectionId}", Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();

            if (!string.IsNullOrEmpty(userId) && _connections.TryRemove(userId, out _))
            {
                _logger.LogInformation("Authenticated user disconnected: {UserId}", userId);
            }
            else
            {
                _logger.LogInformation("Anonymous user disconnected: {ConnectionId}", Context.ConnectionId);
            }

            if (exception != null)
            {
                _logger.LogWarning(exception, "Disconnection exception for ConnectionId: {ConnectionId}", Context.ConnectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        #region Helper Methods

        private string? GetUserId()
        {
            return Context.UserIdentifier ??
                   Context.User?.FindFirst("nameid")?.Value ??
                   Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                   Context.User?.FindFirst("sub")?.Value ??
                   Context.User?.FindFirst("userId")?.Value;
        }

        private string? GetUserName()
        {
            return Context.User?.FindFirst("unique_name")?.Value ??
                   Context.User?.FindFirst(ClaimTypes.Name)?.Value ??
                   Context.User?.FindFirst("name")?.Value ??
                   Context.User?.FindFirst("username")?.Value;
        }

        private string? GetUserEmail()
        {
            return Context.User?.FindFirst("email")?.Value ??
                   Context.User?.FindFirst(ClaimTypes.Email)?.Value;
        }

        private string? GetUserRole()
        {
            return Context.User?.FindFirst("role")?.Value ??
                   Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        }

        private void LogUserInfo(string methodName)
        {
            _logger.LogDebug("=== {MethodName} Debug Info === ConnectionId: {ConnectionId}, UserIdentifier: {UserIdentifier}, IsAuthenticated: {IsAuth}",
                methodName,
                Context.ConnectionId,
                Context.UserIdentifier,
                Context.User?.Identity?.IsAuthenticated);

            if (Context.User?.Claims?.Any() == true)
            {
                foreach (var claim in Context.User.Claims)
                {
                    _logger.LogDebug("  Claim {Type}: {Value}", claim.Type, claim.Value);
                }
            }

            var userId = GetUserId();
            var userName = GetUserName();
            _logger.LogDebug("Extracted UserId: {UserId}, UserName: {UserName}", userId ?? "NULL", userName ?? "NULL");
        }

        #endregion
    }
}