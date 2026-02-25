using Domain.Common;

namespace Domain.Entities.BookingAggregate
{
    /// <summary>
    /// Represents a refund request within a booking.
    /// Created when customer requests refund; status updated by admin approval.
    /// </summary>
    public class Refund : BaseEntity
    {
        public Guid BookingId { get; private set; }
        public decimal RefundAmount { get; private set; }
        public decimal RefundPercentage { get; private set; }
        public string Reason { get; private set; } = string.Empty;
        public bool IsProcessed { get; private set; }
        public DateTime RequestedAt { get; private set; }
        public DateTime? ProcessedAt { get; private set; }

        // EF Core constructor
        private Refund() { }

        public Refund(Guid bookingId, decimal refundAmount, decimal refundPercentage, string reason)
        {
            BookingId = bookingId;
            RefundAmount = refundAmount;
            RefundPercentage = refundPercentage;
            Reason = reason;
            IsProcessed = false;
            RequestedAt = DateTime.UtcNow;
        }

        public void MarkAsProcessed()
        {
            IsProcessed = true;
            ProcessedAt = DateTime.UtcNow;
        }
    }
}
