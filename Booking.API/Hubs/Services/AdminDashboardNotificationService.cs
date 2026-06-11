using Booking.API.Client;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Booking.API.Data;
using Booking.API.Hubs;
using Booking.API.Hubs.Builders;
using Booking.API.Hubs.Interfaces;
using Booking.API.Mappers;
using Booking.API.Services;
using IMovieApiClient = Movie.API.Client.Client.IMovieApiClient;
using DomainBookingStatus = Booking.API.Entities.BookingStatus;

namespace Booking.API.Hubs.Services;

public class AdminDashboardNotificationService : IAdminDashboardNotificationService
{
    private readonly BookingDbContext _dbContext;
    private readonly IMovieApiClient _movieApiClient;
    private readonly IHubContext<AdminDashboardHub, IAdminDashboardHubClient> _hubContext;
    private readonly ILogger<AdminDashboardNotificationService> _logger;

    public AdminDashboardNotificationService(
        BookingDbContext dbContext,
        IMovieApiClient movieApiClient,
        IHubContext<AdminDashboardHub, IAdminDashboardHubClient> hubContext,
        ILogger<AdminDashboardNotificationService> logger)
    {
        _dbContext = dbContext;
        _movieApiClient = movieApiClient;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task PublishBookingCompletedAsync(
        Guid bookingId,
        decimal amount,
        string customerName,
        DateTime occurredAtUtc,
        CancellationToken cancellationToken = default)
    {
        var booking = await _dbContext.Bookings
            .Include(item => item.BookingSeats)
            .FirstOrDefaultAsync(item => item.Id == bookingId, cancellationToken);

        if (booking is null)
        {
            _logger.LogWarning("Cannot publish dashboard activity. Booking {BookingId} not found.", bookingId);
            return;
        }

        if (booking.Status is not DomainBookingStatus.Confirmed and not DomainBookingStatus.CheckedIn)
        {
            _logger.LogInformation(
                "Skipping dashboard activity broadcast for booking {BookingId} because status is {Status}.",
                bookingId,
                booking.Status);
            return;
        }

        var showtimeLookupResponse = await _movieApiClient.LookupShowtimesAsync(
            new Movie.API.Client.ShowtimeLookupRequest { ShowtimeIds = [booking.ShowtimeId] },
            cancellationToken);
        var showtimeLookup = showtimeLookupResponse.Success && showtimeLookupResponse.Data is not null
            ? showtimeLookupResponse.Data
                .Select(ExternalClientDtoMapper.ToBookingShowtimeLookup)
                .FirstOrDefault()
            : null;

        if (showtimeLookup is null)
        {
            _logger.LogWarning("Cannot publish dashboard activity. Showtime {ShowtimeId} not found.", booking.ShowtimeId);
            return;
        }

        var activity = new DashboardRecentActivityDto
        {
            BookingId = booking.Id,
            ShowtimeId = booking.ShowtimeId,
            MovieId = showtimeLookup.MovieId,
            MovieTitle = showtimeLookup.MovieTitle,
            CustomerName = string.IsNullOrWhiteSpace(customerName) ? booking.UserId : customerName,
            Amount = amount,
            SeatsCount = booking.BookingSeats.Count,
            Status = "Completed",
            OccurredAtUtc = occurredAtUtc
        };

        await _hubContext.Clients
            .Group(HubGroupNameBuilder.ForAdminDashboard())
            .NewBooking(activity);
    }
}
