using Cinema.Contracts.Events;

namespace Booking.API.Notifications.Email;

public interface IBookingConfirmationEmailService
{
    Task SendPaymentCompletedEmailAsync(
        PaymentCompletedEvent message,
        CancellationToken cancellationToken = default);
}
