namespace Movie.API.Infrastructure.Integrations.Clients;

public interface ICinemaApiClient
{
    Task<bool> ValidateCinemaHallExistsAsync(Guid cinemaHallId);
    Task<CinemaHallInfo?> GetCinemaHallInfoAsync(Guid cinemaHallId);
    Task<CinemaInfo?> GetCinemaInfoAsync(Guid cinemaId);
    Task<List<CinemaHallInfo>> GetCinemaHallsByCinemaIdAsync(Guid cinemaId);
}

public class CinemaHallInfo
{
    public Guid Id { get; set; }
    public Guid CinemaId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CinemaName { get; set; } = string.Empty;
    public int TotalSeats { get; set; }
}

public class CinemaInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}


