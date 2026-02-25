namespace Domain.Entities.BookingAggregate.Enums
{
    public enum BookingStatus
    {
        Pending       = 1,  // Created, awaiting payment (has expiry)
        Completed     = 2,  // Payment confirmed
        Cancelled     = 3,  // Cancelled by user or expired
        PendingRefund = 4,  // Refund requested, awaiting admin approval
        Refunded      = 5,  // Refund approved and processed
    }
}
