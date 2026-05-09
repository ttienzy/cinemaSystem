using Booking.API.Infrastructure.Hubs.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Booking.API.Infrastructure.Hubs;

/// <summary>
/// SignalR Hub for real-time booking status updates
/// Clients join booking-specific groups to receive notifications when payment is processed
/// </summary>
[Authorize]
public class BookingHub : Hub<IBookingHubClient>
{
    private readonly ILogger<BookingHub> _logger;

    public BookingHub(ILogger<BookingHub> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Join a booking group to receive real-time status updates
    /// </summary>
    /// <param name="bookingId">The booking to monitor</param>
    public async Task JoinBookingGroup(Guid bookingId)
    {
        var userId = Context.User?.Identity?.Name ?? "anonymous";
        var connectionId = Context.ConnectionId;

        _logger.LogInformation(
            "User {UserId} (connection {ConnectionId}) joining booking group {BookingId}",
            userId, connectionId, bookingId);

        try
        {
            var groupName = GetBookingGroupName(bookingId);
            await Groups.AddToGroupAsync(connectionId, groupName);

            _logger.LogInformation(
                "User {UserId} successfully joined booking group {BookingId}",
                userId, bookingId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error adding user {UserId} to booking group {BookingId}",
                userId, bookingId);
            throw new HubException("Failed to join booking group. Please try again.");
        }
    }

    /// <summary>
    /// Leave a booking group
    /// </summary>
    /// <param name="bookingId">The booking to leave</param>
    public async Task LeaveBookingGroup(Guid bookingId)
    {
        var userId = Context.User?.Identity?.Name ?? "anonymous";
        var connectionId = Context.ConnectionId;

        _logger.LogInformation(
            "User {UserId} (connection {ConnectionId}) leaving booking group {BookingId}",
            userId, connectionId, bookingId);

        try
        {
            var groupName = GetBookingGroupName(bookingId);
            await Groups.RemoveFromGroupAsync(connectionId, groupName);

            _logger.LogInformation(
                "User {UserId} left booking group {BookingId}",
                userId, bookingId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error removing user {UserId} from booking group {BookingId}",
                userId, bookingId);
        }
    }

    /// <summary>
    /// Called when a client connects
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.Identity?.Name ?? "anonymous";
        var connectionId = Context.ConnectionId;

        _logger.LogInformation(
            "BookingHub client connected: User {UserId}, Connection {ConnectionId}",
            userId, connectionId);

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.Identity?.Name ?? "anonymous";
        var connectionId = Context.ConnectionId;

        if (exception != null)
        {
            _logger.LogWarning(exception,
                "BookingHub client disconnected with error: User {UserId}, Connection {ConnectionId}",
                userId, connectionId);
        }
        else
        {
            _logger.LogInformation(
                "BookingHub client disconnected: User {UserId}, Connection {ConnectionId}",
                userId, connectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Gets the SignalR group name for a booking
    /// </summary>
    private static string GetBookingGroupName(Guid bookingId)
    {
        return $"booking-{bookingId}";
    }
}
