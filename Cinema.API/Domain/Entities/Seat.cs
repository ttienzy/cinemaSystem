using Cinema.Shared.Entities;

namespace Cinema.API.Domain.Entities;

public class Seat : BaseEntity
{
    public Guid CinemaHallId { get; set; }
    public CinemaHall CinemaHall { get; set; } = null!;

    public string Row { get; set; } = string.Empty;
    public int Number { get; set; }
}



