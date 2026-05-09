using Cinema.Shared.Entities;

namespace Cinema.API.Domain.Entities;

public class CinemaHall : BaseEntity
{
    public Guid CinemaId { get; set; }
    public Cinema Cinema { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public int TotalSeats { get; set; }

    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
}


