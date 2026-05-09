using Cinema.Shared.Models;
using System.Text.Json;

namespace Movie.API.Infrastructure.Integrations.Clients;

public class CinemaApiClient : ICinemaApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CinemaApiClient> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CinemaApiClient(HttpClient httpClient, ILogger<CinemaApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> ValidateCinemaHallExistsAsync(Guid cinemaHallId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/cinema-halls/{cinemaHallId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating cinema hall {CinemaHallId}", cinemaHallId);
            return false;
        }
    }

    public async Task<CinemaHallInfo?> GetCinemaHallInfoAsync(Guid cinemaHallId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/cinema-halls/{cinemaHallId}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Cinema hall {CinemaHallId} not found", cinemaHallId);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<CinemaHallDto>>(content, JsonOptions);

            if (apiResponse?.Data == null)
            {
                return null;
            }

            return new CinemaHallInfo
            {
                Id = apiResponse.Data.Id,
                CinemaId = apiResponse.Data.CinemaId,
                Name = apiResponse.Data.Name,
                CinemaName = apiResponse.Data.CinemaName,
                TotalSeats = apiResponse.Data.TotalSeats
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cinema hall info {CinemaHallId}", cinemaHallId);
            return null;
        }
    }

    public async Task<CinemaInfo?> GetCinemaInfoAsync(Guid cinemaId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/cinemas/{cinemaId}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Cinema {CinemaId} not found", cinemaId);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<CinemaDto>>(content, JsonOptions);

            return apiResponse?.Data == null
                ? null
                : new CinemaInfo
                {
                    Id = apiResponse.Data.Id,
                    Name = apiResponse.Data.Name
                };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cinema info {CinemaId}", cinemaId);
            return null;
        }
    }

    public async Task<List<CinemaHallInfo>> GetCinemaHallsByCinemaIdAsync(Guid cinemaId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/cinema-halls/cinema/{cinemaId}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Cinema halls for cinema {CinemaId} not found", cinemaId);
                return [];
            }

            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<CinemaHallDto>>>(content, JsonOptions);

            return apiResponse?.Data?.Select(hall => new CinemaHallInfo
            {
                Id = hall.Id,
                CinemaId = hall.CinemaId,
                Name = hall.Name,
                CinemaName = hall.CinemaName,
                TotalSeats = hall.TotalSeats
            }).ToList() ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cinema halls for cinema {CinemaId}", cinemaId);
            return [];
        }
    }
}

public class CinemaHallDto
{
    public Guid Id { get; set; }
    public Guid CinemaId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CinemaName { get; set; } = string.Empty;
    public int TotalSeats { get; set; }
}

public class CinemaDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}


