using Booking.API.Application.DTOs.External;
using Cinema.Shared.Models;
using System.Net;
using System.Text.Json;

namespace Booking.API.Infrastructure.Integrations.Clients;

/// <summary>
/// Typed HttpClient for Cinema.API calls.
/// Base address configured in appsettings.json ServiceUrls:CinemaApi
/// </summary>
public class CinemaApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CinemaApiClient> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CinemaApiClient(HttpClient httpClient, ILogger<CinemaApiClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<SeatDto>> GetSeatsByCinemaHallIdAsync(Guid cinemaHallId)
    {
        var url = $"/api/cinema-halls/{cinemaHallId}/seats";

        try
        {
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var seats = ApiResponseJsonHelper.DeserializeApiResponse<List<SeatDto>>(content, JsonOptions) ?? [];

                _logger.LogInformation("Retrieved {Count} seats for cinema hall {HallId}",
                    seats.Count, cinemaHallId);

                return seats;
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Cinema hall {HallId} not found", cinemaHallId);
                return new List<SeatDto>();
            }

            _logger.LogError("Failed to get seats for cinema hall {HallId}. Status: {Status}",
                cinemaHallId, response.StatusCode);

            return new List<SeatDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Cinema.API to get seats for hall {HallId}", cinemaHallId);
            throw new ExternalServiceException($"Failed to get seats from Cinema.API: {ex.Message}", ex);
        }
    }

    public async Task<CinemaHallDto?> GetCinemaHallByIdAsync(Guid cinemaHallId)
    {
        var url = $"/api/cinema-halls/{cinemaHallId}";

        try
        {
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var hall = ApiResponseJsonHelper.DeserializeApiResponse<CinemaHallDetailDto>(content, JsonOptions);

                if (hall == null)
                {
                    _logger.LogWarning(
                        "Cinema.API returned empty cinema hall payload for {HallId}. Raw response: {Response}",
                        cinemaHallId,
                        content);
                    return null;
                }

                _logger.LogInformation("Retrieved cinema hall {HallId}", cinemaHallId);
                return new CinemaHallDto
                {
                    Id = hall.Id,
                    CinemaId = hall.CinemaId,
                    Name = hall.Name,
                    TotalSeats = hall.TotalSeats
                };
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Cinema hall {HallId} not found", cinemaHallId);
                return null;
            }

            _logger.LogError("Failed to get cinema hall {HallId}. Status: {Status}",
                cinemaHallId, response.StatusCode);

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Cinema.API to get hall {HallId}", cinemaHallId);
            throw new ExternalServiceException($"Failed to get cinema hall from Cinema.API: {ex.Message}", ex);
        }
    }

    public async Task<List<CinemaHallDto>> GetCinemaHallsByIdsAsync(IEnumerable<Guid> cinemaHallIds)
    {
        var ids = cinemaHallIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (ids.Count == 0)
        {
            return [];
        }

        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/cinema-halls/lookup", new
            {
                CinemaHallIds = ids
            });

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return ApiResponseJsonHelper.DeserializeApiResponse<List<CinemaHallDto>>(content, JsonOptions) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Cinema.API to lookup {Count} cinema halls", ids.Count);
            throw new ExternalServiceException($"Failed to lookup cinema halls from Cinema.API: {ex.Message}", ex);
        }
    }
}


