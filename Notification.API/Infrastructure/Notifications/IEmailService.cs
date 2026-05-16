namespace Notification.API.Infrastructure.Notifications;

public interface IEmailService
{
    Task SendPaymentCompletedAsync(PaymentCompletedEmail email, CancellationToken cancellationToken);
}
