using Domain.Common;
using Domain.Entities.ShowtimeAggregate.Enum;
using Domain.Events;

namespace Domain.Entities.ShowtimeAggregate
{
    /// <summary>
    /// Showtime aggregate root — represents a movie screening at a specific screen/time.
    /// Tracks seat capacity and raises events on status changes.
    /// </summary>
    public class Showtime : BaseEntity, IAggregateRoot
    {
        public Guid CinemaId { get; private set; }
        public Guid MovieId { get; private set; }
        public Guid ScreenId { get; private set; }
        public Guid SlotId { get; private set; }
        public Guid PricingTierId { get; private set; }
        public DateTime ShowDate { get; private set; }
        public DateTime ActualStartTime { get; private set; }
        public DateTime ActualEndTime { get; private set; }
        public ShowtimeStatus Status { get; private set; }

        // ── New: seat capacity tracking ──────────────────────────────
        public int TotalSeats { get; private set; }
        public int BookedSeats { get; private set; }

        private readonly List<ShowtimePricing> _showtimePricings = [];
        public IReadOnlyCollection<ShowtimePricing> ShowtimePricings => _showtimePricings.AsReadOnly();

        // EF Core constructor
        private Showtime() { }

        // ── Factory Method ───────────────────────────────────────────
        public static Showtime Schedule(
            Guid cinemaId, Guid movieId, Guid screenId,
            Guid slotId, Guid pricingTierId,
            DateTime showDate, DateTime startTime, DateTime endTime,
            int totalSeats)
        {
            if (endTime <= startTime)
                throw new DomainException("End time must be after start time.");
            if (showDate.Date < DateTime.UtcNow.Date)
                throw new DomainException("Show date cannot be in the past.");

            return new Showtime
            {
                CinemaId = cinemaId,
                MovieId = movieId,
                ScreenId = screenId,
                SlotId = slotId,
                PricingTierId = pricingTierId,
                ShowDate = showDate,
                ActualStartTime = startTime,
                ActualEndTime = endTime,
                Status = ShowtimeStatus.Scheduled,
                TotalSeats = totalSeats,
                BookedSeats = 0
            };
        }

        // ── Commands ────────────────────────────────────────────────
        public void UpdateShowtime(
            Guid cinemaId, Guid movieId, Guid screenId,
            Guid slotId, Guid pricingTierId,
            DateTime showDate, DateTime startTime, DateTime endTime,
            ShowtimeStatus status)
        {
            if (Status == ShowtimeStatus.Cancelled)
                throw new DomainException("Cannot update a cancelled showtime.");

            CinemaId = cinemaId;
            MovieId = movieId;
            ScreenId = screenId;
            SlotId = slotId;
            PricingTierId = pricingTierId;
            ShowDate = showDate;
            ActualStartTime = startTime;
            ActualEndTime = endTime;
            Status = status;
        }

        public void Confirm()
        {
            if (Status != ShowtimeStatus.Scheduled)
                throw new DomainException("Only scheduled showtimes can be confirmed.");
            Status = ShowtimeStatus.Confirmed;
        }

        public void Cancel(string movieTitle)
        {
            if (Status == ShowtimeStatus.Cancelled)
                throw new DomainException("Showtime is already cancelled.");

            Status = ShowtimeStatus.Cancelled;
            Raise(new ShowtimeCancelledEvent(Id, CinemaId, movieTitle, ShowDate));
        }

        public void IncrementBookedSeats(int count = 1)
        {
            BookedSeats += count;
            if (BookedSeats > TotalSeats)
                throw new DomainException("Booked seats cannot exceed total seats.");
        }

        public void DecrementBookedSeats(int count = 1)
        {
            BookedSeats = Math.Max(0, BookedSeats - count);
        }

        public int AvailableSeats => TotalSeats - BookedSeats;

        // ── Pricing management ────────────────────────────────────
        public void AddShowtimePricing(ShowtimePricing pricing)
            => _showtimePricings.Add(pricing);

        public void AddRangeShowtimePricing(IEnumerable<ShowtimePricing> pricings)
            => _showtimePricings.AddRange(pricings);

        public ShowtimePricing? GetPricingBySeatType(Guid seatTypeId)
            => _showtimePricings.FirstOrDefault(p => p.SeatTypeId == seatTypeId);

        public bool RemoveShowtimePricing(Guid pricingId)
        {
            var pricing = _showtimePricings.FirstOrDefault(p => p.Id == pricingId);
            return pricing is not null && _showtimePricings.Remove(pricing);
        }

        public List<ShowtimePricing> GetAllShowtimePricings() => [.. _showtimePricings];

        // Legacy compatibility
        public void UpdateStatus(ShowtimeStatus status) => Status = status;
        public void MarkAsCancelled() => Status = ShowtimeStatus.Cancelled;
        public void MarkAsConfirmed() => Status = ShowtimeStatus.Confirmed;

        /// <summary>Legacy constructor for backward compatibility with old Infrastructure services.</summary>
        public Showtime(Guid cinemaId, Guid movieId, Guid screenId, Guid slotId, Guid pricingTierId,
            DateTime showDate, DateTime startTime, DateTime endTime, ShowtimeStatus status)
        {
            CinemaId = cinemaId;
            MovieId = movieId;
            ScreenId = screenId;
            SlotId = slotId;
            PricingTierId = pricingTierId;
            ShowDate = showDate;
            ActualStartTime = startTime;
            ActualEndTime = endTime;
            Status = status;
            TotalSeats = 0;
            BookedSeats = 0;
        }

    }
}
