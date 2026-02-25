using Domain.Common;

namespace Domain.Events
{
    /// <summary>Raised when a new booking is created and awaiting payment.</summary>
    public class BookingCreatedEvent : BaseDomainEvent
    {
        public Guid BookingId { get; }
        public Guid CustomerId { get; }
        public Guid ShowtimeId { get; }
        public decimal TotalAmount { get; }
        public List<Guid> SeatIds { get; }
        public DateTime ExpiresAt { get; }

        public BookingCreatedEvent(
            Guid bookingId, Guid customerId, Guid showtimeId,
            decimal totalAmount, List<Guid> seatIds, DateTime expiresAt)
        {
            BookingId = bookingId;
            CustomerId = customerId;
            ShowtimeId = showtimeId;
            TotalAmount = totalAmount;
            SeatIds = seatIds;
            ExpiresAt = expiresAt;
        }
    }
}
