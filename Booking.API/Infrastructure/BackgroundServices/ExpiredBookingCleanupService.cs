using Booking.API.Infrastructure.Persistence;
using Booking.API.Domain.Entities;
using BookingEntity = Booking.API.Domain.Entities.Booking;
using Cinema.EventBus.Abstractions;
using Cinema.EventBus.Events;
using Microsoft.EntityFrameworkCore;

namespace Booking.API.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that periodically cleans up expired bookings
/// </summary>
public class ExpiredBookingCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExpiredBookingCleanupService> _logger;
    private readonly IConfiguration _configuration;
    private readonly TimeSpan _cleanupInterval;

    public ExpiredBookingCleanupService(
        IServiceProvider serviceProvider,
        ILogger<ExpiredBookingCleanupService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        // Get cleanup interval from configuration (default: 1 minute)
        var intervalMinutes = _configuration.GetValue<int>("BackgroundServices:CleanupIntervalMinutes", 1);
        _cleanupInterval = TimeSpan.FromMinutes(intervalMinutes);

        _logger.LogInformation("ExpiredBookingCleanupService initialized with interval: {Interval} minutes",
            intervalMinutes);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ExpiredBookingCleanupService is starting");

        // Wait a bit before starting to allow other services to initialize
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredBookingsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cleaning up expired bookings");
            }

            // Wait for the next cleanup cycle
            await Task.Delay(_cleanupInterval, stoppingToken);
        }

        _logger.LogInformation("ExpiredBookingCleanupService is stopping");
    }

    private async Task CleanupExpiredBookingsAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Starting expired bookings cleanup cycle");

        using var scope = _serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
        var seatStatusService = scope.ServiceProvider.GetRequiredService<ISeatStatusService>();
        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

        try
        {
            // Find all Pending bookings that have expired
            var now = DateTime.UtcNow;
            var expiredBookings = await dbContext.Bookings
                .Include(b => b.BookingSeats)
                .Where(b => b.Status == BookingStatus.Pending
                         && b.ExpiresAt.HasValue
                         && b.ExpiresAt.Value <= now)
                .ToListAsync(cancellationToken);

            if (!expiredBookings.Any())
            {
                _logger.LogDebug("No expired bookings found");
                return;
            }

            _logger.LogInformation("Found {Count} expired bookings to clean up", expiredBookings.Count);

            var successCount = 0;
            var failureCount = 0;

            foreach (var booking in expiredBookings)
            {
                try
                {
                    await ProcessExpiredBookingAsync(
                        booking,
                        seatStatusService,
                        eventBus,
                        cancellationToken);

                    successCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to process expired booking {BookingId}",
                        booking.Id);
                    failureCount++;
                }
            }

            // Save all changes to database
            await dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Cleanup cycle completed. Success: {Success}, Failed: {Failed}",
                successCount,
                failureCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cleanup cycle");
            throw;
        }
    }

    private async Task ProcessExpiredBookingAsync(
        BookingEntity booking,
        ISeatStatusService seatStatusService,
        IEventBus eventBus,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing expired booking {BookingId} for user {UserId}, expired at {ExpiresAt}",
            booking.Id,
            booking.UserId,
            booking.ExpiresAt);

        // 1. Update booking status to Expired
        booking.Status = BookingStatus.Expired;

        // 2. Release seats in Redis
        var seatIds = booking.BookingSeats.Select(bs => bs.SeatId).ToList();

        try
        {
            var released = await seatStatusService.ReleaseBookedSeatsAsync(
                booking.ShowtimeId,
                seatIds);

            if (released)
            {
                _logger.LogInformation(
                    "Released {Count} seats for expired booking {BookingId}",
                    seatIds.Count,
                    booking.Id);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to release some seats for expired booking {BookingId}",
                    booking.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error releasing seats for expired booking {BookingId}",
                booking.Id);
            // Continue anyway - we still want to mark the booking as expired
        }

        // 3. Publish BookingExpiredIntegrationEvent
        try
        {
            var expiredEvent = new BookingExpiredIntegrationEvent(
                booking.Id,
                booking.ShowtimeId,
                seatIds,
                DateTime.UtcNow);

            eventBus.Publish(expiredEvent);

            _logger.LogInformation(
                "Published BookingExpiredIntegrationEvent for booking {BookingId}",
                booking.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error publishing expired event for booking {BookingId}",
                booking.Id);
            // Continue anyway - booking is still marked as expired
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ExpiredBookingCleanupService is stopping gracefully");
        await base.StopAsync(cancellationToken);
    }
}



