namespace Booking.API.Application.DTOs.External;

/// <summary>
/// Cinema hall data from Cinema.API
/// </summary>
public class CinemaHallDto
{
    public Guid Id { get; set; }
    public Guid CinemaId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TotalSeats { get; set; }
}

/// <summary>
/// Detailed cinema hall payload returned by Cinema.API GET /api/cinema-halls/{id}
/// </summary>
public class CinemaHallDetailDto : CinemaHallDto
{
    public string CinemaName { get; set; } = string.Empty;
    public List<SeatDto> Seats { get; set; } = [];
}


