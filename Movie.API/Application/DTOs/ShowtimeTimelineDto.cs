namespace Movie.API.Application.DTOs;

public class ShowtimeTimelineDto
{
    public Guid CinemaId { get; set; }
    public string CinemaName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public DateTime TimelineStart { get; set; }
    public DateTime TimelineEnd { get; set; }
    public int CleaningBufferMinutes { get; set; }
    public List<TimelineRoomDto> Rooms { get; set; } = new();
}

public class TimelineRoomDto
{
    public Guid RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public int TotalSeats { get; set; }
    public List<TimelineShowtimeDto> Showtimes { get; set; } = new();
}

public class TimelineShowtimeDto
{
    public Guid Id { get; set; }
    public Guid MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public DateTime CleaningEnd { get; set; }
    public int DurationMinutes { get; set; }
    public int CleaningBufferMinutes { get; set; }
    public decimal Price { get; set; }
    public int TotalSeats { get; set; }
    public int BookedSeats { get; set; }
    public decimal OccupancyRate { get; set; }
    public bool HasBookings { get; set; }
    public bool CanReschedule { get; set; }
}
