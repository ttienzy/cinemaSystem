using Booking.API.Infrastructure.Hubs.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Booking.API.Infrastructure.Hubs.Services;

/// <summary>
/// SignalR-based implementation of seat notification service
/// Handles broadcasting to showtime-specific groups with error handling
/// </summary>
public class SeatNotificationService : ISeatNotificationService
{
    private readonly IHubContext<SeatHub, ISeatHubClient> _hubContext;
    private readonly ILogger<SeatNotificationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly bool _enableBroadcasts;

    public SeatNotificationService(
        IHubContext<SeatHub, ISeatHubClient> hubContext,
        ILogger<SeatNotificationService> logger,
        IConfiguration configuration)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        // Feature flag for enabling/disabling broadcasts
        _enableBroadcasts = _configuration.GetValue<bool>("Features:EnableSignalRBroadcasts", true);
    }

    public async Task NotifySeatLockedAsync(
        Guid showtimeId,
        List<Guid> seatIds,
        string userId,
        DateTime lockedUntil)
    {
        if (!_enableBroadcasts)
        {
            _logger.LogDebug("SignalR broadcasts disabled, skipping seat locked notification");
            return;
        }

        try
        {
            var notification = new SeatStatusChangedNotification
            {
                ShowtimeId = showtimeId,
                SeatIds = seatIds,
                Status = "locked",
                UserId = userId,
                LockedUntil = lockedUntil,
                Timestamp = DateTime.UtcNow
            };

            var groupName = GetShowtimeGroupName(showtimeId);
            await _hubContext.Clients
                .Group(groupName)
                .SeatLocked(notification);

            _logger.LogInformation(
                "Broadcasted seat locked notification for showtime {ShowtimeId}: {SeatCount} seats by user {UserId}",
                showtimeId, seatIds.Count, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to broadcast seat locked notification for showtime {ShowtimeId}",
                showtimeId);
            // Don't throw - broadcast failure shouldn't fail the operation
        }
    }

    public async Task NotifySeatUnlockedAsync(
        Guid showtimeId,
        List<Guid> seatIds)
    {
        if (!_enableBroadcasts)
        {
            _logger.LogDebug("SignalR broadcasts disabled, skipping seat unlocked notification");
            return;
        }

        try
        {
            var notification = new SeatStatusChangedNotification
            {
                ShowtimeId = showtimeId,
                SeatIds = seatIds,
                Status = "available",
                Timestamp = DateTime.UtcNow
            };

            var groupName = GetShowtimeGroupName(showtimeId);
            await _hubContext.Clients
                .Group(groupName)
                .SeatUnlocked(notification);

            _logger.LogInformation(
                "Broadcasted seat unlocked notification for showtime {ShowtimeId}: {SeatCount} seats",
                showtimeId, seatIds.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to broadcast seat unlocked notification for showtime {ShowtimeId}",
                showtimeId);
            // Don't throw - broadcast failure shouldn't fail the operation
        }
    }

    public async Task NotifySeatBookedAsync(
        Guid showtimeId,
        List<Guid> seatIds)
    {
        if (!_enableBroadcasts)
        {
            _logger.LogDebug("SignalR broadcasts disabled, skipping seat booked notification");
            return;
        }

        try
        {
            var notification = new SeatStatusChangedNotification
            {
                ShowtimeId = showtimeId,
                SeatIds = seatIds,
                Status = "booked",
                Timestamp = DateTime.UtcNow
            };

            var groupName = GetShowtimeGroupName(showtimeId);
            await _hubContext.Clients
                .Group(groupName)
                .SeatBooked(notification);

            _logger.LogInformation(
                "Broadcasted seat booked notification for showtime {ShowtimeId}: {SeatCount} seats",
                showtimeId, seatIds.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to broadcast seat booked notification for showtime {ShowtimeId}",
                showtimeId);
            // Don't throw - broadcast failure shouldn't fail the operation
        }
    }

    public async Task NotifySeatReleasedAsync(
        Guid showtimeId,
        List<Guid> seatIds)
    {
        if (!_enableBroadcasts)
        {
            _logger.LogDebug("SignalR broadcasts disabled, skipping seat released notification");
            return;
        }

        try
        {
            var notification = new SeatStatusChangedNotification
            {
                ShowtimeId = showtimeId,
                SeatIds = seatIds,
                Status = "available",
                Timestamp = DateTime.UtcNow
            };

            var groupName = GetShowtimeGroupName(showtimeId);
            await _hubContext.Clients
                .Group(groupName)
                .SeatReleased(notification);

            _logger.LogInformation(
                "Broadcasted seat released notification for showtime {ShowtimeId}: {SeatCount} seats",
                showtimeId, seatIds.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to broadcast seat released notification for showtime {ShowtimeId}",
                showtimeId);
            // Don't throw - broadcast failure shouldn't fail the operation
        }
    }

    private static string GetShowtimeGroupName(Guid showtimeId)
    {
        return $"showtime:{showtimeId}";
    }
}
