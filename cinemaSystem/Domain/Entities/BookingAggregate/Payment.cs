using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.BookingAggregate
{
    public class Payment : BaseEntity
    {
        public Guid BookingId { get; private set; }
        public string? PaymentMethod { get; private set; } // e.g., cash, card, ewallet, bank_transfer
        public string? PaymentProvider { get; private set; } // e.g., Visa, Momo, ZaloPay
        public decimal Amount { get; private set; }
        public string? Currency { get; private set; }
        public string? TransactionId { get; private set; }
        public string? ReferenceCode { get; private set; }
        public DateTime PaymentTime { get; private set; }
        public Payment()
        {
            Currency = "VND";
            PaymentTime = DateTime.UtcNow;
        }
        public Payment(decimal amount)
        {
            PaymentMethod = "VnPay";
            PaymentProvider = "VnPay";
            Amount = amount;
            Currency = "VND";
            PaymentTime = DateTime.UtcNow;
        }
        public void UpdatePayment(string paymentMethod, string transactionId, string referenceCode)
        {
            PaymentMethod = paymentMethod;
            TransactionId = transactionId;
            ReferenceCode = referenceCode;
        }

    }
}
