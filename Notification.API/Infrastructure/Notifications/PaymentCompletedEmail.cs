namespace Notification.API.Infrastructure.Notifications;

public class PaymentCompletedEmail
{
    public Guid BookingId { get; init; }
    public Guid PaymentId { get; init; }
    public string TransactionId { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTime CompletedAt { get; init; }
    public string CustomerEmail { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
}
