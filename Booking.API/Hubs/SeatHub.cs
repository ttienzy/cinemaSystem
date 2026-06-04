using Booking.API.Hubs.Builders;
using Booking.API.Hubs.Constants;
using Booking.API.Hubs.Extensions;
using Booking.API.Hubs.Interfaces;
using Booking.API.Hubs.Models;
using Booking.API.Hubs.Services;
using Booking.API.Infrastructure.Hubs.Models;
using IMovieApiClient = Movie.API.Client.Client.IMovieApiClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Booking.API.Hubs;

/// <summary>
/// SignalR Hub for real-time seat availability updates
/// Clients join showtime-specific groups to receive targeted notifications
/// </summary>
[Authorize]
public class SeatHub : Hub<ISeatHubClient>
{
    private readonly IMovieApiClient _movieApiClient;
    private readonly ILogger<SeatHub> _logger;
    private readonly IConnectionTracker _connectionTracker;

    public SeatHub(
        IMovieApiClient movieApiClient,
        ILogger<SeatHub> logger,
        IConnectionTracker connectionTracker)
    {
        _movieApiClient = movieApiClient ?? throw new ArgumentNullException(nameof(movieApiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _connectionTracker = connectionTracker ?? throw new ArgumentNullException(nameof(connectionTracker));
    }

    /// <summary>
    /// Join a showtime group to receive real-time seat updates
    /// </summary>
    /// <param name="showtimeId">The showtime to monitor</param>
    public async Task<HubOperationResult> JoinShowtime(Guid showtimeId)
    {
        var userId = Context.GetUserIdOrAnonymous();
        var connectionId = Context.ConnectionId;

        _logger.LogInformation(
            "User {UserId} (connection {ConnectionId}) joining showtime {ShowtimeId}",
            userId, connectionId, showtimeId);

        try
        {
            // Validate showtime exists and is active
            var showtimeResponse = await _movieApiClient.GetShowtimeByIdAsync(showtimeId);
            if (!showtimeResponse.Success || showtimeResponse.Data is null)
            {
                _logger.LogWarning(
                    "User {UserId} attempted to join non-existent showtime {ShowtimeId}",
                    userId, showtimeId);
                return HubOperationResult.Fail(
                    HubOperationErrorCodes.ShowtimeNotFound,
                    $"Showtime {showtimeId} not found");
            }

            // Add to SignalR group
            var groupName = HubGroupNameBuilder.ForShowtime(showtimeId);
            await Groups.AddToGroupAsync(connectionId, groupName);

            // Track connection
            await _connectionTracker.AddConnectionAsync(showtimeId, connectionId, userId);

            // Notify all clients in group about updated viewer count
            var viewerCount = await _connectionTracker.GetViewerCountAsync(showtimeId);
            await Clients.Group(groupName).ViewerCountUpdated(new ViewerCountNotification
            {
                ShowtimeId = showtimeId,
                ViewerCount = viewerCount
            });

            _logger.LogInformation(
                "User {UserId} successfully joined showtime {ShowtimeId}. Current viewers: {ViewerCount}",
                userId, showtimeId, viewerCount);

            return HubOperationResult.Ok("Joined showtime successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error adding user {UserId} to showtime {ShowtimeId}",
                userId, showtimeId);
            return HubOperationResult.Fail(
                HubOperationErrorCodes.JoinShowtimeFailed,
                "Failed to join showtime. Please try again.");
        }
    }

    /// <summary>
    /// Leave a showtime group
    /// </summary>
    /// <param name="showtimeId">The showtime to leave</param>
    public async Task<HubOperationResult> LeaveShowtime(Guid showtimeId)
    {
        var userId = Context.GetUserIdOrAnonymous();
        var connectionId = Context.ConnectionId;

        _logger.LogInformation(
            "User {UserId} (connection {ConnectionId}) leaving showtime {ShowtimeId}",
            userId, connectionId, showtimeId);

        try
        {
            var groupName = HubGroupNameBuilder.ForShowtime(showtimeId);
            await Groups.RemoveFromGroupAsync(connectionId, groupName);

            // Untrack connection
            await _connectionTracker.RemoveConnectionAsync(showtimeId, connectionId);

            // Notify remaining clients about updated viewer count
            var viewerCount = await _connectionTracker.GetViewerCountAsync(showtimeId);
            await Clients.Group(groupName).ViewerCountUpdated(new ViewerCountNotification
            {
                ShowtimeId = showtimeId,
                ViewerCount = viewerCount
            });

            _logger.LogInformation(
                "User {UserId} left showtime {ShowtimeId}. Remaining viewers: {ViewerCount}",
                userId, showtimeId, viewerCount);

            return HubOperationResult.Ok("Left showtime successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error removing user {UserId} from showtime {ShowtimeId}",
                userId, showtimeId);
            return HubOperationResult.Fail(
                HubOperationErrorCodes.LeaveShowtimeFailed,
                "Failed to leave showtime. Please try again.");
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
            "SignalR client connected: User {UserId}, Connection {ConnectionId}",
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
                "SignalR client disconnected with error: User {UserId}, Connection {ConnectionId}",
                userId, connectionId);
        }
        else
        {
            _logger.LogInformation(
                "SignalR client disconnected: User {UserId}, Connection {ConnectionId}",
                userId, connectionId);
        }

        // Clean up all showtime groups this connection was in
        await _connectionTracker.RemoveConnectionFromAllShowtimesAsync(connectionId);

        await base.OnDisconnectedAsync(exception);
    }
}
