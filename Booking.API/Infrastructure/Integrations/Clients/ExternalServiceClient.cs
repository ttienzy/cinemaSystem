using Booking.API.Application.DTOs.External;

namespace Booking.API.Infrastructure.Integrations.Clients;

/// <summary>
/// Facade that delegates to typed HttpClients (CinemaApiClient, MovieApiClient).
/// Maintains backward compatibility with existing code that depends on IExternalServiceClient.
/// </summary>
public class ExternalServiceClient : IExternalServiceClient
{
    private readonly CinemaApiClient _cinemaApiClient;
    private readonly MovieApiClient _movieApiClient;

    public ExternalServiceClient(
        CinemaApiClient cinemaApiClient,
        MovieApiClient movieApiClient)
    {
        _cinemaApiClient = cinemaApiClient ?? throw new ArgumentNullException(nameof(cinemaApiClient));
        _movieApiClient = movieApiClient ?? throw new ArgumentNullException(nameof(movieApiClient));
    }

    // Cinema.API calls — delegated to CinemaApiClient
    public Task<List<SeatDto>> GetSeatsByCinemaHallIdAsync(Guid cinemaHallId)
        => _cinemaApiClient.GetSeatsByCinemaHallIdAsync(cinemaHallId);

    public Task<CinemaHallDto?> GetCinemaHallByIdAsync(Guid cinemaHallId)
        => _cinemaApiClient.GetCinemaHallByIdAsync(cinemaHallId);

    // Movie.API calls — delegated to MovieApiClient
    public Task<ShowtimeDto?> GetShowtimeByIdAsync(Guid showtimeId)
        => _movieApiClient.GetShowtimeByIdAsync(showtimeId);

    public Task<MovieDto?> GetMovieByIdAsync(Guid movieId)
        => _movieApiClient.GetMovieByIdAsync(movieId);
}

/// <summary>
/// Exception thrown when external service call fails
/// </summary>
public class ExternalServiceException : Exception
{
    public ExternalServiceException(string message) : base(message) { }
    public ExternalServiceException(string message, Exception innerException) : base(message, innerException) { }
}


