using Booking.API.Infrastructure.Hubs.Builders;
using Booking.API.Infrastructure.Hubs.Constants;
using Booking.API.Infrastructure.Hubs.Extensions;
using Booking.API.Infrastructure.Hubs.Interfaces;
using Booking.API.Infrastructure.Hubs.Models;
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
    public async Task<HubOperationResult> JoinBookingGroup(Guid bookingId)
    {
        var userId = Context.GetUserIdOrAnonymous();
        var connectionId = Context.ConnectionId;

        _logger.LogInformation(
            "User {UserId} (connection {ConnectionId}) joining booking group {BookingId}",
            userId, connectionId, bookingId);

        try
        {
            var groupName = HubGroupNameBuilder.ForBooking(bookingId);
            await Groups.AddToGroupAsync(connectionId, groupName);

            _logger.LogInformation(
                "User {UserId} successfully joined booking group {BookingId}",
                userId, bookingId);

            return HubOperationResult.Ok("Joined booking group successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error adding user {UserId} to booking group {BookingId}",
                userId, bookingId);
            return HubOperationResult.Fail(
                HubOperationErrorCodes.JoinBookingGroupFailed,
                "Failed to join booking group. Please try again.");
        }
    }

    /// <summary>
    /// Leave a booking group
    /// </summary>
    /// <param name="bookingId">The booking to leave</param>
    public async Task<HubOperationResult> LeaveBookingGroup(Guid bookingId)
    {
        var userId = Context.GetUserIdOrAnonymous();
        var connectionId = Context.ConnectionId;

        _logger.LogInformation(
            "User {UserId} (connection {ConnectionId}) leaving booking group {BookingId}",
            userId, connectionId, bookingId);

        try
        {
            var groupName = HubGroupNameBuilder.ForBooking(bookingId);
            await Groups.RemoveFromGroupAsync(connectionId, groupName);

            _logger.LogInformation(
                "User {UserId} left booking group {BookingId}",
                userId, bookingId);

            return HubOperationResult.Ok("Left booking group successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error removing user {UserId} from booking group {BookingId}",
                userId, bookingId);
            return HubOperationResult.Fail(
                HubOperationErrorCodes.LeaveBookingGroupFailed,
                "Failed to leave booking group. Please try again.");
        }
    }

    /// <summary>
    /// Called when a client connects
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.GetUserIdOrAnonymous();
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
        var userId = Context.GetUserIdOrAnonymous();
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
}
