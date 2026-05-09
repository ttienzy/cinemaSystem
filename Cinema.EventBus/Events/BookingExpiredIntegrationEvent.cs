using Cinema.EventBus.Abstractions;

namespace Cinema.EventBus.Events;

/// <summary>
/// Event published when a booking expires (payment timeout)
/// </summary>
public class BookingExpiredIntegrationEvent : IntegrationEvent
{
    public Guid BookingId { get; set; }
    public Guid ShowtimeId { get; set; }
    public List<Guid> SeatIds { get; set; } = new();
    public DateTime ExpiredAt { get; set; }

    public BookingExpiredIntegrationEvent()
    {
    }

    public BookingExpiredIntegrationEvent(
        Guid bookingId,
        Guid showtimeId,
        List<Guid> seatIds,
        DateTime expiredAt)
    {
        BookingId = bookingId;
        ShowtimeId = showtimeId;
        SeatIds = seatIds;
        ExpiredAt = expiredAt;
    }
}
