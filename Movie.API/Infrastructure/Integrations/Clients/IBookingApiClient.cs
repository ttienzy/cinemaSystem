namespace Movie.API.Infrastructure.Integrations.Clients;

public interface IBookingApiClient
{
    Task<Dictionary<Guid, int>> GetShowtimeOccupancyAsync(List<Guid> showtimeIds);
}
