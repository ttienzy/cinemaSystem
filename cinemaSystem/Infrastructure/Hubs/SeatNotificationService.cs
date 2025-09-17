using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Hubs
{
    public class SeatNotificationService : ISeatNotificationService
    {
        private readonly IHubContext<SeatHub> _hubContext;
        private readonly ILogger<SeatNotificationService> _logger;

        public SeatNotificationService(
            IHubContext<SeatHub> hubContext,
            ILogger<SeatNotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }
        public async Task NotifySeatReservedAsync(string showtimeId, string[] seatIds)
        {
            try
            {
                var notificationData = new
                {
                    EventType = "Reserved",
                    ShowtimeId = showtimeId,
                    SeatIds = seatIds,
                };

                // Notify all clients in showtime group
                await _hubContext.Clients
                    .Group($"{showtimeId}")
                    .SendAsync("SeatReserved", notificationData);

                _logger.LogInformation("Notified seat reservation: ShowtimeId={ShowtimeId}, SeatIds={SeatIds}",
                    showtimeId, string.Join(",", seatIds));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to notify seat reservation: ShowtimeId={ShowtimeId}, SeatIds={SeatIds}",
                    showtimeId, string.Join(",", seatIds));
            }
        }

        public async Task NotifySeatReleasedAsync(string showtimeId, string[] seatIds)
        {
            try
            {
                var notificationData = new
                {
                    EventType = "SeatReleased",
                    ShowtimeId = showtimeId,
                    SeatIds = seatIds,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients
                    .Group($"{showtimeId}")
                    .SendAsync("SeatReleased", notificationData);

                _logger.LogInformation("Notified seat release: ShowtimeId={ShowtimeId}, SeatIds={SeatIds}",
                    showtimeId, string.Join(",", seatIds));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to notify seat release: ShowtimeId={ShowtimeId}, SeatIds={SeatIds}",
                    showtimeId, string.Join(",", seatIds));
            }
        }

        public async Task NotifySeatSoldAsync(string showtimeId, string[] seatIds)
        {
            try
            {
                var notificationData = new
                {
                    EventType = "SeatSold",
                    ShowtimeId = showtimeId,
                    SeatIds = seatIds,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients
                    .Group($"{showtimeId}")
                    .SendAsync("SeatSold", notificationData);

                _logger.LogInformation("Notified seat sold: ShowtimeId={ShowtimeId}, SeatIds={SeatIds}",
                    showtimeId, string.Join(",", seatIds));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to notify seat sold: ShowtimeId={ShowtimeId}, SeatIds={SeatIds}",
                    showtimeId, string.Join(",", seatIds));
            }
        }

    }

    // Extension Methods for easier usage
    public static class SeatNotificationExtensions
    {
        public static async Task NotifySeatStatusBatchAsync(
            this ISeatNotificationService notificationService,
            string showtimeId,
            Dictionary<string[], string> seatStatusUpdates) // seatIds -> status
        {
            var tasks = new List<Task>();

            foreach (var (seatIds, status) in seatStatusUpdates)
            {
                var task = status.ToLower() switch
                {
                    "reserved" => notificationService.NotifySeatReservedAsync(showtimeId, seatIds),
                    "available" => notificationService.NotifySeatReleasedAsync(showtimeId, seatIds),
                    "sold" => notificationService.NotifySeatSoldAsync(showtimeId, seatIds),
                    _ => Task.CompletedTask
                };
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        public static async Task NotifyWithRetryAsync(
            this ISeatNotificationService notificationService,
            Func<Task> notificationAction,
            int maxRetries = 3)
        {
            var retryCount = 0;
            while (retryCount < maxRetries)
            {
                try
                {
                    await notificationAction();
                    return;
                }
                catch (Exception)
                {
                    retryCount++;
                    if (retryCount >= maxRetries) throw;
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount))); // Exponential backoff
                }
            }
        }
    }
}
