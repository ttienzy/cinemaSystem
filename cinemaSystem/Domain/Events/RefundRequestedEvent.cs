using Domain.Common;

namespace Domain.Events
{
    /// <summary>Raised when a refund is requested by the customer.</summary>
    public class RefundRequestedEvent : BaseDomainEvent
    {
        public Guid BookingId { get; }
        public Guid CustomerId { get; }
        public decimal RefundAmount { get; }
        public decimal RefundPercentage { get; }
        public Guid ShowtimeId { get; }

        public RefundRequestedEvent(
            Guid bookingId, Guid customerId,
            decimal refundAmount, decimal refundPercentage,
            Guid showtimeId)
        {
            BookingId = bookingId;
            CustomerId = customerId;
            RefundAmount = refundAmount;
            RefundPercentage = refundPercentage;
            ShowtimeId = showtimeId;
        }
    }
}
