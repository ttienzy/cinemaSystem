using Booking.API.Application.DTOs.External;
using Cinema.Shared.Models;
using System.Net;
using System.Text.Json;

namespace Booking.API.Infrastructure.Integrations.Clients;

/// <summary>
/// Typed HttpClient for Movie.API calls.
/// Base address configured in appsettings.json ServiceUrls:MovieApi
/// </summary>
public class MovieApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MovieApiClient> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public MovieApiClient(HttpClient httpClient, ILogger<MovieApiClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ShowtimeDto?> GetShowtimeByIdAsync(Guid showtimeId)
    {
        var url = $"/api/showtimes/{showtimeId}";

        try
        {
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var showtime = ApiResponseJsonHelper.DeserializeApiResponse<ShowtimeDto>(content, JsonOptions);

                if (showtime == null)
                {
                    _logger.LogWarning(
                        "Movie.API returned empty showtime payload for {ShowtimeId}. Raw response: {Response}",
                        showtimeId,
                        content);
                    return null;
                }

                _logger.LogInformation("Retrieved showtime {ShowtimeId}", showtimeId);
                return showtime;
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Showtime {ShowtimeId} not found", showtimeId);
                return null;
            }

            _logger.LogError("Failed to get showtime {ShowtimeId}. Status: {Status}",
                showtimeId, response.StatusCode);

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Movie.API to get showtime {ShowtimeId}", showtimeId);
            throw new ExternalServiceException($"Failed to get showtime from Movie.API: {ex.Message}", ex);
        }
    }

    public async Task<MovieDto?> GetMovieByIdAsync(Guid movieId)
    {
        var url = $"/api/movies/{movieId}";

        try
        {
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var movie = ApiResponseJsonHelper.DeserializeApiResponse<MovieDto>(content, JsonOptions);

                if (movie == null)
                {
                    _logger.LogWarning(
                        "Movie.API returned empty movie payload for {MovieId}. Raw response: {Response}",
                        movieId,
                        content);
                    return null;
                }

                _logger.LogInformation("Retrieved movie {MovieId}", movieId);
                return movie;
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Movie {MovieId} not found", movieId);
                return null;
            }

            _logger.LogError("Failed to get movie {MovieId}. Status: {Status}",
                movieId, response.StatusCode);

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Movie.API to get movie {MovieId}", movieId);
            throw new ExternalServiceException($"Failed to get movie from Movie.API: {ex.Message}", ex);
        }
    }

    public async Task<List<ShowtimeLookupDto>> GetShowtimeLookupsByIdsAsync(IEnumerable<Guid> showtimeIds)
    {
        var ids = showtimeIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (ids.Count == 0)
        {
            return [];
        }

        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/showtimes/lookup", new
            {
                ShowtimeIds = ids
            });

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return ApiResponseJsonHelper.DeserializeApiResponse<List<ShowtimeLookupDto>>(content, JsonOptions) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Movie.API to lookup {Count} showtimes", ids.Count);
            throw new ExternalServiceException($"Failed to lookup showtimes from Movie.API: {ex.Message}", ex);
        }
    }

    public async Task<List<ShowtimeLookupDto>> GetShowtimesByRangeAsync(DateTime from, DateTime to)
    {
        var fromQuery = Uri.EscapeDataString(from.ToString("O"));
        var toQuery = Uri.EscapeDataString(to.ToString("O"));
        var url = $"/api/showtimes/range?from={fromQuery}&to={toQuery}";

        try
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return ApiResponseJsonHelper.DeserializeApiResponse<List<ShowtimeLookupDto>>(content, JsonOptions) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Movie.API to get showtimes in range {From} - {To}", from, to);
            throw new ExternalServiceException($"Failed to get showtimes by range from Movie.API: {ex.Message}", ex);
        }
    }
}


