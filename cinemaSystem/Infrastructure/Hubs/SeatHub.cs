using Application.Interfaces.Integrations;
using Application.Interfaces.Persistences;
using Infrastructure.Hubs.Constants;
using Infrastructure.Redis.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Shared.Models.DataModels.ShowtimeDtos;
using System.Security.Claims;

namespace Infrastructure.Hubs
{
    public class SeatHub : Hub
    {
        private static readonly Dictionary<string, string> _connections = new Dictionary<string, string>();
        private readonly ICacheService _cacheService;
        private readonly IShowtimeService _showtimeService;

        public SeatHub(ICacheService cacheService, IShowtimeService showtimeService)
        {
            _cacheService = cacheService;
            _showtimeService = showtimeService;
        }

        public async Task JoinShowtimeGroup(Guid showtimeId)
        {
            var connectionId = Context.ConnectionId;

            // Debug user info
            LogUserInfo("JoinShowtimeGroup");

            await Groups.AddToGroupAsync(connectionId, showtimeId.ToString());
            await Clients.Group(showtimeId.ToString()).SendAsync(SignalMethodConstants.JoinShowtimeGroup, $"A client join room: {showtimeId}");
        }

        public async Task LeaveShowtimeGroup(Guid showtimeId)
        {
            var connectionId = Context.ConnectionId;

            // Debug user info
            LogUserInfo("LeaveShowtimeGroup");

            await Groups.RemoveFromGroupAsync(connectionId, showtimeId.ToString());
            await Clients.Group(showtimeId.ToString()).SendAsync(SignalMethodConstants.LeaveShowtimeGroup, $"A client left room: {showtimeId}");
        }

        //[Authorize] // Bật authorization cho method này
        public async Task SeatsReserved(Guid showtimeId, List<Guid> seatIds)
        {
            try
            {
                // Debug user info
                LogUserInfo("SeatsReserved");

                // Check if user is authenticated
                //if (!Context.User?.Identity?.IsAuthenticated ?? true)
                //{
                //    await Clients.Caller.SendAsync("AuthenticationRequired", "You must be logged in to reserve seats.");
                //    return;
                //}

                //var userId = GetUserId();
                //if (string.IsNullOrEmpty(userId))
                //{
                //    await Clients.Caller.SendAsync("AuthenticationError", "Unable to identify user.");
                //    return;
                //}

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

                //Console.WriteLine($"User {userId} reserved seats: {string.Join(", ", seatIds)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SeatsReserved] Error: {ex.Message} | StackTrace: {ex.StackTrace}");
                await Clients.Caller.SendAsync("SeatsReservedError", "An error occurred while reserving seats.");
            }
        }

        public async Task SeatsReleased(Guid showtimeId, List<Guid> seatIds)
        {
            try
            {
                // Debug user info
                //LogUserInfo("SeatsReleased");

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
                Console.WriteLine($"User {userId ?? "Anonymous"} released seats: {string.Join(", ", seatIds)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SeatsReleased] Error: {ex.Message} | StackTrace: {ex.StackTrace}");
                await Clients.Caller.SendAsync("SeatsReleasedError", "An error occurred while releasing seats.");
            }
        }

        public override async Task OnConnectedAsync()
        {
            LogUserInfo("OnConnectedAsync");

            var userId = GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                Console.WriteLine($"Authenticated user connected: {userId} with ConnectionId: {Context.ConnectionId}");

                // Store user connection mapping
                if (!_connections.ContainsKey(userId))
                {
                    _connections.Add(userId, Context.ConnectionId);
                }
                else
                {
                    _connections[userId] = Context.ConnectionId; // Update if user reconnects
                }
            }
            else
            {
                Console.WriteLine($"Anonymous user connected: {Context.ConnectionId}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();

            if (!string.IsNullOrEmpty(userId) && _connections.ContainsKey(userId))
            {
                _connections.Remove(userId);
                Console.WriteLine($"Authenticated user disconnected: {userId}");
            }
            else
            {
                Console.WriteLine($"Anonymous user disconnected: {Context.ConnectionId}");
            }

            if (exception != null)
            {
                Console.WriteLine($"Disconnection exception: {exception.Message}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        #region Helper Methods

        private string? GetUserId()
        {
            // Dựa trên token của bạn, userId nằm trong "nameid" claim
            return Context.UserIdentifier ??
                   Context.User?.FindFirst("nameid")?.Value ??           // Từ token của bạn
                   Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                   Context.User?.FindFirst("sub")?.Value ??
                   Context.User?.FindFirst("userId")?.Value;
        }

        private string? GetUserName()
        {
            // Dựa trên token của bạn, username nằm trong "unique_name" claim
            return Context.User?.FindFirst("unique_name")?.Value ??      // Từ token của bạn
                   Context.User?.FindFirst(ClaimTypes.Name)?.Value ??
                   Context.User?.FindFirst("name")?.Value ??
                   Context.User?.FindFirst("username")?.Value;
        }

        private string? GetUserEmail()
        {
            return Context.User?.FindFirst("email")?.Value ??            // Từ token của bạn
                   Context.User?.FindFirst(ClaimTypes.Email)?.Value;
        }

        private string? GetUserRole()
        {
            return Context.User?.FindFirst("role")?.Value ??             // Từ token của bạn
                   Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        }

        private void LogUserInfo(string methodName)
        {
            Console.WriteLine($"\n=== {methodName} Debug Info ===");
            Console.WriteLine($"ConnectionId: {Context.ConnectionId}");
            Console.WriteLine($"UserIdentifier: {Context.UserIdentifier}");
            Console.WriteLine($"IsAuthenticated: {Context.User?.Identity?.IsAuthenticated}");
            Console.WriteLine($"AuthenticationType: {Context.User?.Identity?.AuthenticationType}");
            Console.WriteLine($"User Claims Count: {Context.User?.Claims?.Count() ?? 0}");

            if (Context.User?.Claims?.Any() == true)
            {
                Console.WriteLine("Claims:");
                foreach (var claim in Context.User.Claims)
                {
                    Console.WriteLine($"  {claim.Type}: {claim.Value}");
                }
            }

            var userId = GetUserId();
            var userName = GetUserName();
            Console.WriteLine($"Extracted UserId: {userId ?? "NULL"}");
            Console.WriteLine($"Extracted UserName: {userName ?? "NULL"}");
            Console.WriteLine("==============================\n");
        }

        #endregion
    }
}