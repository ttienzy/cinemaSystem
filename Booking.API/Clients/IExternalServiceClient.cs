using Booking.API.Client;

namespace Booking.API.Clients;

/// <summary>
/// Client for calling external microservices (Cinema.API, Movie.API)
/// </summary>
public interface IExternalServiceClient
{
    // Cinema.API calls
    Task<List<SeatDto>> GetSeatsByCinemaHallIdAsync(Guid cinemaHallId);
    Task<CinemaHallDto?> GetCinemaHallByIdAsync(Guid cinemaHallId);

    // Movie.API calls
    Task<ShowtimeDto?> GetShowtimeByIdAsync(Guid showtimeId);
    Task<MovieDto?> GetMovieByIdAsync(Guid movieId);
}


