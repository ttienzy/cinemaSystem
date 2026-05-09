namespace Booking.API.Application.DTOs.External;

/// <summary>
/// Seat data from Cinema.API
/// </summary>
public class SeatDto
{
    public Guid Id { get; set; }
    public Guid CinemaHallId { get; set; }
    public string Row { get; set; } = string.Empty;
    public int Number { get; set; }
}


