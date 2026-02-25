using Domain.Common;
using Domain.Entities.CinemaAggregate;

namespace Domain.Services
{
    /// <summary>
    /// Stateless domain service that enforces booking creation rules.
    /// Rules:
    ///   1. Cannot book within 1 hour of showtime start
    ///   2. Maximum 8 tickets per booking
    ///   3. Must have enough available seats
    ///   4. All selected seats must be bookable (active + not blocked)
    ///   5. Couple seats must be booked as a pair
    /// </summary>
    public static class BookingRules
    {
        public const int MaxTicketsPerBooking = 8;
        public const int MinHoursBeforeShowtime = 1;
        public const int SeatLockMinutes = 15;

        public static void Validate(
            DateTime showtimeStart,
            int availableSeats,
            List<Seat> selectedSeats)
        {
            // Rule 1: Time check
            var hoursLeft = (showtimeStart - DateTime.UtcNow).TotalHours;
            if (hoursLeft < MinHoursBeforeShowtime)
                throw new DomainException(
                    $"Bookings must be made at least {MinHoursBeforeShowtime} hour before showtime.");

            // Rule 2: Ticket count limit
            if (selectedSeats.Count > MaxTicketsPerBooking)
                throw new DomainException(
                    $"Maximum {MaxTicketsPerBooking} tickets allowed per booking.");

            // Rule 3: Enough available seats
            if (selectedSeats.Count > availableSeats)
                throw new DomainException(
                    $"Not enough available seats. Only {availableSeats} remaining.");

            // Rule 4: All seats must be bookable
            foreach (var seat in selectedSeats)
            {
                if (!seat.IsBookable())
                    throw new DomainException(
                        $"Seat {seat.SeatLabel} is not available for booking.");
            }

            // Rule 5: Couple seats must be booked in pairs
            ValidateCoupleSeats(selectedSeats);
        }

        /// <summary>Lightweight time check — usable from Application handlers without Seat entities.</summary>
        public static void ValidateBookingTime(DateTime showtimeStart)
        {
            var hoursLeft = (showtimeStart - DateTime.UtcNow).TotalHours;
            if (hoursLeft < MinHoursBeforeShowtime)
                throw new DomainException(
                    $"Bookings must be made at least {MinHoursBeforeShowtime} hour before showtime.");
        }

        /// <summary>Lightweight ticket count check.</summary>
        public static void ValidateTicketCount(int count)
        {
            if (count > MaxTicketsPerBooking)
                throw new DomainException(
                    $"Maximum {MaxTicketsPerBooking} tickets allowed per booking.");
            if (count <= 0)
                throw new DomainException("At least 1 ticket is required.");
        }

        private static void ValidateCoupleSeats(List<Seat> selectedSeats)
        {
            var selectedIds = selectedSeats.Select(s => s.Id).ToHashSet();

            foreach (var seat in selectedSeats.Where(s => s.LinkedSeatNumber.HasValue))
            {
                // Find the linked partner within the same row
                var partner = selectedSeats.FirstOrDefault(s =>
                    s.RowName == seat.RowName
                    && s.Number == seat.LinkedSeatNumber!.Value
                    && s.Id != seat.Id);

                if (partner == null || !selectedIds.Contains(partner.Id))
                    throw new DomainException(
                        $"Couple seat {seat.SeatLabel} must be booked together with its pair.");
            }
        }
    }
}
