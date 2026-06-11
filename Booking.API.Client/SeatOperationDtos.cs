namespace Booking.API.Client;

public class SeatLockResult
{
    public bool Success { get; set; }
    public List<Guid> LockedSeats { get; set; } = new();
    public List<Guid> AlreadyLockedSeats { get; set; } = new();
    public string? Message { get; set; }
}

public class SeatBookingResult
{
    public bool Success { get; set; }
    public List<Guid> BookedSeats { get; set; } = new();
    public List<Guid> FailedSeats { get; set; } = new();
    public string? Message { get; set; }
    public SeatBookingFailureReason? FailureReason { get; set; }
}

public enum SeatBookingFailureReason
{
    NotLocked = 0,
    LockExpired = 1,
    WrongUser = 2,
    AlreadyBooked = 3,
    Unavailable = 4
}

public class SeatStatusInfo
{
    public Guid SeatId { get; set; }
    public SeatStatus Status { get; set; }
    public string? UserId { get; set; }
    public Guid? BookingId { get; set; }
    public DateTime? LockedUntil { get; set; }
}
