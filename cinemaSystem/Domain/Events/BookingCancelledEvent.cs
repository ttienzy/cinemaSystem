using Domain.Common;

namespace Domain.Events
{
    /// <summary>Raised when a booking is cancelled (before payment or due to expiry).</summary>
    public class BookingCancelledEvent : BaseDomainEvent
    {
        public Guid BookingId { get; }
        public Guid CustomerId { get; }
        public Guid ShowtimeId { get; }
        public string Reason { get; }
        public List<Guid> SeatIds { get; }

        public BookingCancelledEvent(
            Guid bookingId, Guid customerId, Guid showtimeId,
            string reason, List<Guid> seatIds)
        {
            BookingId = bookingId;
            CustomerId = customerId;
            ShowtimeId = showtimeId;
            Reason = reason;
            SeatIds = seatIds;
        }
    }
}
