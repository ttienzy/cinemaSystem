namespace Payment.API.Domain.Entities;

public static class PaymentEntityExtensions
{
    public static void ApplyStatusUpdate(
        this PaymentEntity payment,
        PaymentStatus status,
        string? transactionId = null,
        string? paymentMethod = null,
        DateTime? completedAt = null,
        string? gatewayMetadata = null)
    {
        payment.Status = status;

        if (!string.IsNullOrWhiteSpace(transactionId))
        {
            payment.TransactionId = transactionId;
        }

        if (!string.IsNullOrWhiteSpace(paymentMethod))
        {
            payment.PaymentMethod = paymentMethod;
        }

        if (completedAt.HasValue)
        {
            payment.CompletedAt = completedAt.Value;
        }

        if (!string.IsNullOrWhiteSpace(gatewayMetadata))
        {
            payment.GatewayMetadata = gatewayMetadata;
        }

        payment.UpdatedAt = DateTime.UtcNow;
    }
}
