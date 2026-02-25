using Domain.Common;

namespace Domain.Events
{
    /// <summary>Raised when payment is confirmed and booking is complete.</summary>
    public class BookingCompletedEvent : BaseDomainEvent
    {
        public Guid BookingId { get; }
        public Guid CustomerId { get; }
        public Guid ShowtimeId { get; }
        public decimal FinalAmount { get; }
        public string BookingCode { get; }
        public List<Guid> SeatIds { get; }

        public BookingCompletedEvent(
            Guid bookingId, Guid customerId, Guid showtimeId,
            decimal finalAmount, string bookingCode, List<Guid> seatIds)
        {
            BookingId = bookingId;
            CustomerId = customerId;
            ShowtimeId = showtimeId;
            FinalAmount = finalAmount;
            BookingCode = bookingCode;
            SeatIds = seatIds;
        }
    }
}
