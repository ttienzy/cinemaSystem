
namespace Booking.API.Infrastructure.Caching.Models;

public class SeatAvailabilityResponse
{
    public Guid ShowtimeId { get; set; }
    public Guid CinemaHallId { get; set; }
    public string CinemaHallName { get; set; } = string.Empty;
    public List<SeatStatusDto> Seats { get; set; } = [];
    public SeatAvailabilitySummary Summary { get; set; } = new();

    public static implicit operator Booking.API.Client.SeatAvailabilityResponse(SeatAvailabilityResponse response)
        => new()
        {
            ShowtimeId = response.ShowtimeId,
            CinemaHallId = response.CinemaHallId,
            CinemaHallName = response.CinemaHallName,
            Seats = response.Seats.Select(seat => (Booking.API.Client.SeatStatusDto)seat).ToList(),
            Summary = response.Summary
        };
}

public class SeatStatusDto
{
    public Guid SeatId { get; set; }
    public string Row { get; set; } = string.Empty;
    public int Number { get; set; }
    public SeatStatus Status { get; set; }
    public decimal Price { get; set; }
    public string? LockedBy { get; set; }
    public DateTime? LockedUntil { get; set; }

    public static implicit operator Booking.API.Client.SeatStatusDto(SeatStatusDto seat)
        => new()
        {
            SeatId = seat.SeatId,
            Row = seat.Row,
            Number = seat.Number,
            Price = seat.Price,
            Status = (Booking.API.Client.SeatStatus)seat.Status,
            LockedBy = seat.LockedBy,
            LockedUntil = seat.LockedUntil
        };
}

public class SeatAvailabilitySummary
{
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
    public int LockedSeats { get; set; }
    public int BookedSeats { get; set; }

    public static implicit operator Booking.API.Client.SeatAvailabilitySummary(SeatAvailabilitySummary summary)
        => new()
        {
            TotalSeats = summary.TotalSeats,
            AvailableSeats = summary.AvailableSeats,
            LockedSeats = summary.LockedSeats,
            BookedSeats = summary.BookedSeats
        };
}

public class SeatLockResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<Guid> LockedSeats { get; set; } = [];
    public List<Guid> AlreadyLockedSeats { get; set; } = [];

    public static implicit operator Booking.API.Client.SeatLockResult(SeatLockResult result)
        => new()
        {
            Success = result.Success,
            Message = result.Message,
            LockedSeats = result.LockedSeats,
            AlreadyLockedSeats = result.AlreadyLockedSeats
        };
}

public class SeatStatusInfo
{
    public Guid SeatId { get; set; }
    public SeatStatus Status { get; set; }
    public string? UserId { get; set; }
    public Guid? BookingId { get; set; }
    public DateTime? LockedUntil { get; set; }

    public static implicit operator Booking.API.Client.SeatStatusInfo(SeatStatusInfo status)
        => new()
        {
            SeatId = status.SeatId,
            Status = (Booking.API.Client.SeatStatus)status.Status,
            UserId = status.UserId,
            BookingId = status.BookingId,
            LockedUntil = status.LockedUntil
        };
}

public class SeatBookingResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<Guid> BookedSeats { get; set; } = [];
    public List<Guid> FailedSeats { get; set; } = [];
    public SeatBookingFailureReason FailureReason { get; set; }

    public static implicit operator Booking.API.Client.SeatBookingResult(SeatBookingResult result)
        => new()
        {
            Success = result.Success,
            Message = result.Message,
            BookedSeats = result.BookedSeats,
            FailedSeats = result.FailedSeats,
            FailureReason = result.Success || result.FailureReason == SeatBookingFailureReason.None
                ? null
                : MapFailureReason(result.FailureReason)
        };

    private static Booking.API.Client.SeatBookingFailureReason MapFailureReason(SeatBookingFailureReason reason)
        => reason switch
        {
            SeatBookingFailureReason.LockExpired => Booking.API.Client.SeatBookingFailureReason.LockExpired,
            SeatBookingFailureReason.WrongUser => Booking.API.Client.SeatBookingFailureReason.WrongUser,
            SeatBookingFailureReason.AlreadyBooked => Booking.API.Client.SeatBookingFailureReason.AlreadyBooked,
            SeatBookingFailureReason.Unavailable => Booking.API.Client.SeatBookingFailureReason.Unavailable,
            _ => Booking.API.Client.SeatBookingFailureReason.NotLocked
        };
}

public enum SeatBookingFailureReason
{
    None = 0,
    NotLocked = 1,
    LockExpired = 2,
    WrongUser = 3,
    AlreadyBooked = 4,
    Unavailable = 5
}

public class SeatMapMetadata
{
    public Guid ShowtimeId { get; set; }
    public Guid CinemaHallId { get; set; }
    public string CinemaHallName { get; set; } = string.Empty;
}
