using Domain.Common;

namespace Domain.Events
{
    /// <summary>Raised when admin approves a refund and money is being returned.</summary>
    public class BookingRefundedEvent : BaseDomainEvent
    {
        public Guid BookingId { get; }
        public Guid CustomerId { get; }
        public decimal RefundAmount { get; }

        public BookingRefundedEvent(Guid bookingId, Guid customerId, decimal refundAmount)
        {
            BookingId = bookingId;
            CustomerId = customerId;
            RefundAmount = refundAmount;
        }
    }
}
