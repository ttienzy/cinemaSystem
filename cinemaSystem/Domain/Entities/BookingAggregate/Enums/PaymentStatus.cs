namespace Domain.Entities.BookingAggregate.Enums
{
    public enum PaymentStatus
    {
        Pending   = 1,  // Payment URL generated, not yet confirmed
        Completed = 2,  // Payment confirmed by VnPay callback
        Failed    = 3,  // Payment rejected or timed out
        Refunded  = 4,  // Refund processed
    }
}
