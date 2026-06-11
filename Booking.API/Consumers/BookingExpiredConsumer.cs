using Booking.API.Infrastructure.Caching.Services;
using Booking.API.Hubs.Services;
using Cinema.Contracts.Events;
using MassTransit;

namespace Booking.API.Consumers;

public class BookingExpiredConsumer : IConsumer<BookingExpiredEvent>
{
    private readonly ISeatStatusService _seatStatusService;
    private readonly ISeatNotificationService _seatNotificationService;
    private readonly ILogger<BookingExpiredConsumer> _logger;

    public BookingExpiredConsumer(
        ISeatStatusService seatStatusService,
        ISeatNotificationService seatNotificationService,
        ILogger<BookingExpiredConsumer> logger)
    {
        _seatStatusService = seatStatusService ?? throw new ArgumentNullException(nameof(seatStatusService));
        _seatNotificationService = seatNotificationService ?? throw new ArgumentNullException(nameof(seatNotificationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Consume(ConsumeContext<BookingExpiredEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "MassTransit consumed BookingExpiredEvent for booking {BookingId}",
            message.BookingId);

        var released = await _seatStatusService.ReleaseSeatsForBookingAsync(
            message.ShowtimeId,
            message.SeatIds,
            message.BookingId);

        if (!released)
        {
            _logger.LogWarning(
                "Some Redis seats could not be released for expired booking {BookingId}",
                message.BookingId);
            return;
        }

        await _seatNotificationService.NotifySeatReleasedAsync(message.ShowtimeId, message.SeatIds);
    }
}
