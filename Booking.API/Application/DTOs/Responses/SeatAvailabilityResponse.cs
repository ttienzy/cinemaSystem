namespace Booking.API.Application.DTOs.Responses;

/// <summary>
/// Response containing seat availability for a showtime
/// </summary>
public class SeatAvailabilityResponse
{
    public Guid ShowtimeId { get; set; }
    public Guid CinemaHallId { get; set; }
    public string CinemaHallName { get; set; } = string.Empty;
    public List<SeatStatusDto> Seats { get; set; } = new();
    public SeatAvailabilitySummary Summary { get; set; } = new();
}

public class SeatStatusDto
{
    public Guid SeatId { get; set; }
    public string Row { get; set; } = string.Empty;
    public int Number { get; set; }
    public decimal Price { get; set; }
    public SeatStatus Status { get; set; }
    public string? LockedBy { get; set; }
    public DateTime? LockedUntil { get; set; }
}

public class SeatAvailabilitySummary
{
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
    public int LockedSeats { get; set; }
    public int BookedSeats { get; set; }
}

public enum SeatStatus
{
    Available = 0,
    Locked = 1,
    Booked = 2,
    Unavailable = 3
}


