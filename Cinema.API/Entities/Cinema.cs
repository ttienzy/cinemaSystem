using Cinema.API.Client;

namespace Cinema.API.Entities;

public class Cinema
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? City { get; set; }

    public ICollection<CinemaHall> CinemaHalls { get; set; } = new List<CinemaHall>();

    public static Cinema Create(string name, string address, string? city)
    {
        var cinema = new Cinema();
        cinema.UpdateDetails(name, address, city);

        return cinema;
    }

    public void UpdateDetails(string name, string address, string? city)
    {
        Name = name;
        Address = address;
        City = city;
    }

    public bool HasCinemaHalls()
    {
        return CinemaHalls.Count != 0;
    }

    public int GetTotalHalls()
    {
        return CinemaHalls.Count;
    }

    public int GetTotalSeats()
    {
        return CinemaHalls.Sum(cinemaHall => cinemaHall.TotalSeats);
    }

    public string GetStatus()
    {
        return CinemaHalls.Any(cinemaHall => cinemaHall.HasConfiguredSeatMap())
            ? CinemaStatuses.Active
            : CinemaStatuses.Inactive;
    }

    public bool MatchesStatus(string status)
    {
        return string.Equals(GetStatus(), status, StringComparison.Ordinal);
    }

    public static bool TryNormalizeStatus(string? status, out string? normalizedStatus)
    {
        normalizedStatus = null;

        if (string.IsNullOrWhiteSpace(status))
        {
            return true;
        }

        normalizedStatus = status.Trim().ToLowerInvariant() switch
        {
            "active" => CinemaStatuses.Active,
            "inactive" => CinemaStatuses.Inactive,
            _ => null
        };

        return normalizedStatus is not null;
    }
}


