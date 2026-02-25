using Domain.Common;
using Domain.Entities.BookingAggregate.Enums;

namespace Domain.Entities.BookingAggregate
{
    /// <summary>
    /// Payment entity with full lifecycle status tracking.
    /// </summary>
    public class Payment : BaseEntity
    {
        public Guid BookingId { get; private set; }
        public string PaymentMethod { get; private set; } = string.Empty;
        public string? PaymentProvider { get; private set; }
        public decimal Amount { get; private set; }
        public string Currency { get; private set; } = "VND";
        public string? TransactionId { get; private set; }
        public string? ReferenceCode { get; private set; }
        public PaymentStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }

        // EF Core constructor
        private Payment() { }

        /// <summary>Create a pending payment for a new booking.</summary>
        public static Payment CreatePending(Guid bookingId, decimal amount, string provider = "VnPay")
        {
            return new Payment
            {
                BookingId = bookingId,
                PaymentMethod = provider,
                PaymentProvider = provider,
                Amount = amount,
                Currency = "VND",
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void Complete(string transactionId, string referenceCode)
        {
            Status = PaymentStatus.Completed;
            TransactionId = transactionId;
            ReferenceCode = referenceCode;
            CompletedAt = DateTime.UtcNow;
        }

        public void Fail()
        {
            Status = PaymentStatus.Failed;
        }

        public void MarkRefunded()
        {
            Status = PaymentStatus.Refunded;
        }

        // Legacy compatibility
        public void UpdatePayment(string paymentMethod, string transactionId, string referenceCode)
        {
            PaymentMethod = paymentMethod;
            TransactionId = transactionId;
            ReferenceCode = referenceCode;
            Status = PaymentStatus.Completed;
            CompletedAt = DateTime.UtcNow;
        }
    }
}
