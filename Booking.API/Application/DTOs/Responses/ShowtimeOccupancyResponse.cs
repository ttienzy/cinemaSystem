namespace Booking.API.Application.DTOs.Responses;

public class ShowtimeOccupancyResponse
{
    public List<ShowtimeOccupancyItemResponse> Items { get; set; } = new();
}

public class ShowtimeOccupancyItemResponse
{
    public Guid ShowtimeId { get; set; }
    public int BookedSeats { get; set; }
}
