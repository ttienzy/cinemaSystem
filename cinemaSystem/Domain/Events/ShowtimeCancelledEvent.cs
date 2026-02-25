using Domain.Common;

namespace Domain.Events
{
    /// <summary>Raised when a showtime is cancelled — all bookings must be refunded.</summary>
    public class ShowtimeCancelledEvent : BaseDomainEvent
    {
        public Guid ShowtimeId { get; }
        public Guid CinemaId { get; }
        public string MovieTitle { get; }
        public DateTime ShowDate { get; }

        public ShowtimeCancelledEvent(
            Guid showtimeId, Guid cinemaId, string movieTitle, DateTime showDate)
        {
            ShowtimeId = showtimeId;
            CinemaId = cinemaId;
            MovieTitle = movieTitle;
            ShowDate = showDate;
        }
    }
}
