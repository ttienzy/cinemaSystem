using Cinema.Shared.Entities;

namespace Cinema.API.Domain.Entities;

public class Seat : BaseEntity
{
    public Guid CinemaHallId { get; set; }
    public CinemaHall CinemaHall { get; set; } = null!;

    public string Row { get; set; } = string.Empty;
    public int Number { get; set; }

    public static Seat Create(Guid cinemaHallId, string row, int number)
    {
        var seat = new Seat
        {
            CinemaHallId = cinemaHallId
        };

        seat.UpdatePosition(row, number);
        return seat;
    }

    public void UpdatePosition(string row, int number)
    {
        Row = row;
        Number = number;
        UpdatedAt = DateTime.UtcNow;
    }

    public string GetDisplayName()
    {
        return $"{Row}{Number}";
    }

    public bool MatchesPosition(string row, int number)
    {
        return string.Equals(Row, row, StringComparison.Ordinal) && Number == number;
    }
}



