using Domain.Common;
using Domain.Entities.BookingAggregate;
using Domain.Entities.BookingAggregate.Enums;

namespace Domain.Services
{
    /// <summary>
    /// Stateless domain service that encapsulates the cinema's refund policy.
    /// Policy Rules (based on hours remaining before showtime):
    ///   - More than 24h: 100% refund
    ///   - 12–24h:        80%  refund
    ///   - 4–12h:         50%  refund
    ///   - Less than 4h:  No refund
    ///   - Already checked in: No refund
    /// </summary>
    public static class RefundPolicy
    {
        public static (bool CanRefund, decimal Percentage, string Reason) Evaluate(
            Booking booking, DateTime showtimeStartTime)
        {
            if (booking.Status != BookingStatus.Completed)
                return (false, 0, "Only completed bookings can be refunded.");

            if (booking.IsCheckedIn)
                return (false, 0, "Cannot refund after check-in.");

            var hoursLeft = (showtimeStartTime - DateTime.UtcNow).TotalHours;

            return hoursLeft switch
            {
                > 24 => (true, 100m, "Full refund — more than 24h before showtime."),
                > 12 => (true, 80m,  "80% refund — between 12h and 24h before showtime."),
                > 4  => (true, 50m,  "50% refund — between 4h and 12h before showtime."),
                _    => (false, 0m,  "No refund available — less than 4h before showtime.")
            };
        }
    }
}
