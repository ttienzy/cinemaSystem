namespace Movie.API.Clients;

public interface IBookingApiClient
{
    Task<Dictionary<Guid, int>> GetShowtimeOccupancyAsync(List<Guid> showtimeIds);
}
