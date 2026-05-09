using Cinema.Shared.Entities;

namespace Cinema.API.Domain.Entities;

public class Cinema : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? City { get; set; }

    public ICollection<CinemaHall> CinemaHalls { get; set; } = new List<CinemaHall>();
}


