using Booking.API.Data;
using Booking.API.Entities;
using Booking.API.Hubs.Services;
using Cinema.Contracts.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using BookingEntity = Booking.API.Entities.Booking;

namespace Booking.API.Infrastructure.BackgroundServices;

public class ExpiredBookingCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExpiredBookingCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval;

    public ExpiredBookingCleanupService(
        IServiceProvider serviceProvider,
        ILogger<ExpiredBookingCleanupService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var intervalSeconds = configuration.GetValue<int>(
            "BackgroundServices:ExpiredBookingCleanupIntervalSeconds",
            60);

        _cleanupInterval = TimeSpan.FromSeconds(intervalSeconds);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "ExpiredBookingCleanupService started with interval {IntervalSeconds}s",
            _cleanupInterval.TotalSeconds);

        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredBookingsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired bookings");
            }

            await Task.Delay(_cleanupInterval, stoppingToken);
        }
    }

    private async Task CleanupExpiredBookingsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        var bookingNotificationService = scope.ServiceProvider.GetRequiredService<IBookingNotificationService>();

        var now = DateTime.UtcNow;
        var expiredBookings = await dbContext.Bookings
            .Include(booking => booking.BookingSeats)
            .Where(booking => booking.Status == BookingStatus.Pending
                           && booking.ExpiresAt.HasValue
                           && booking.ExpiresAt.Value <= now)
            .ToListAsync(cancellationToken);

        if (expiredBookings.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Found {Count} expired pending bookings", expiredBookings.Count);

        foreach (var booking in expiredBookings)
        {
            booking.MarkExpired(now);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var booking in expiredBookings)
        {
            await PublishBookingExpiredAsync(publishEndpoint, booking, now, cancellationToken);
            await bookingNotificationService.NotifyBookingFailedAsync(
                booking.Id,
                "Booking expired",
                cancellationToken);
        }
    }

    private async Task PublishBookingExpiredAsync(
        IPublishEndpoint publishEndpoint,
        BookingEntity booking,
        DateTime expiredAt,
        CancellationToken cancellationToken)
    {
        var seatIds = booking.BookingSeats
            .Select(seat => seat.SeatId)
            .ToList();

        await publishEndpoint.Publish(new BookingExpiredEvent
        {
            CorrelationId = booking.Id,
            BookingId = booking.Id,
            UserId = booking.UserId,
            ShowtimeId = booking.ShowtimeId,
            SeatIds = seatIds,
            TotalPrice = booking.TotalPrice,
            ExpiredAt = expiredAt
        }, cancellationToken);

        _logger.LogInformation(
            "Marked booking {BookingId} as expired and published BookingExpiredEvent",
            booking.Id);
    }
}
