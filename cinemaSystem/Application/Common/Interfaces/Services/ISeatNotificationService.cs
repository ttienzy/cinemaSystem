namespace Application.Common.Interfaces.Services
{
    /// <summary>
    /// Abstraction for real-time seat status notifications (e.g., via SignalR).
    /// Infrastructure layer provides the concrete implementation.
    /// </summary>
    public interface ISeatNotificationService
    {
        Task NotifySeatReservedAsync(Guid showtimeId, IEnumerable<Guid> seatIds);
        Task NotifySeatSoldAsync(Guid showtimeId, IEnumerable<Guid> seatIds);
        Task NotifySeatReleasedAsync(Guid showtimeId, IEnumerable<Guid> seatIds);
        Task NotifyBookingExpiredAsync(Guid showtimeId, IEnumerable<Guid> seatIds);
    }
}
