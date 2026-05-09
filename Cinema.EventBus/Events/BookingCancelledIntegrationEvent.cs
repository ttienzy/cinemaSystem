using Cinema.EventBus.Abstractions;

namespace Cinema.EventBus.Events;

/// <summary>
/// Event published when a booking is cancelled
/// </summary>
public class BookingCancelledIntegrationEvent : IntegrationEvent
{
    public Guid BookingId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Guid ShowtimeId { get; set; }
    public List<Guid> SeatIds { get; set; } = new();
    public bool NeedsRefund { get; set; }
    public string Reason { get; set; } = string.Empty;

    public BookingCancelledIntegrationEvent()
    {
    }

    public BookingCancelledIntegrationEvent(
        Guid bookingId,
        string userId,
        Guid showtimeId,
        List<Guid> seatIds,
        bool needsRefund,
        string reason)
    {
        BookingId = bookingId;
        UserId = userId;
        ShowtimeId = showtimeId;
        SeatIds = seatIds;
        NeedsRefund = needsRefund;
        Reason = reason;
    }
}
