namespace Movie.API.Application.DTOs.External;

public class ShowtimeOccupancyResponseDto
{
    public List<ShowtimeOccupancyItemDto> Items { get; set; } = new();
}

public class ShowtimeOccupancyItemDto
{
    public Guid ShowtimeId { get; set; }
    public int BookedSeats { get; set; }
}
