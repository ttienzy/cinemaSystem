namespace Cinema.API.Entities;

public class CinemaHall
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CinemaId { get; set; }
    public Cinema Cinema { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public int TotalSeats { get; set; }

    public ICollection<Seat> Seats { get; set; } = new List<Seat>();

    public static CinemaHall Create(Guid cinemaId, string name)
    {
        return new CinemaHall
        {
            CinemaId = cinemaId,
            Name = name,
            TotalSeats = 0
        };
    }

    public void UpdateName(string name)
    {
        Name = name;
    }

    public bool HasConfiguredSeatMap()
    {
        return Seats.Count != 0;
    }

    public bool HasSeats()
    {
        return TotalSeats > 0 || Seats.Count != 0;
    }

    public void IncreaseTotalSeats(int count = 1)
    {
        TotalSeats += Math.Max(0, count);
    }

    public void DecreaseTotalSeats(int count = 1)
    {
        TotalSeats = Math.Max(0, TotalSeats - Math.Max(0, count));
    }
}


