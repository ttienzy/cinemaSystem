namespace Cinema.API.Application.DTOs;

public class CinemaDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? City { get; set; }
    public string Status { get; set; } = string.Empty;
    public int TotalHalls { get; set; }
    public int TotalSeats { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CinemaDetailDto : CinemaDto
{
    public List<CinemaHallDto> CinemaHalls { get; set; } = new();
}

public class CinemaHallDto
{
    public Guid Id { get; set; }
    public Guid CinemaId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TotalSeats { get; set; }
    public bool SeatMapConfigured { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CinemaHallDetailDto : CinemaHallDto
{
    public string CinemaName { get; set; } = string.Empty;
    public List<SeatDto> Seats { get; set; } = new();
}

public class SeatDto
{
    public Guid Id { get; set; }
    public Guid CinemaHallId { get; set; }
    public string Row { get; set; } = string.Empty;
    public int Number { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}


