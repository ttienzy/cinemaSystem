using Cinema.Shared.Models;
using Movie.API.Application.DTOs.External;
using System.Text;
using System.Text.Json;

namespace Movie.API.Infrastructure.Integrations.Clients;

public class BookingApiClient : IBookingApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BookingApiClient> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public BookingApiClient(HttpClient httpClient, ILogger<BookingApiClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Dictionary<Guid, int>> GetShowtimeOccupancyAsync(List<Guid> showtimeIds)
    {
        if (showtimeIds.Count == 0)
        {
            return new Dictionary<Guid, int>();
        }

        var payload = JsonSerializer.Serialize(new { showtimeIds });
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync("/api/bookings/analytics/showtime-occupancy", content);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<ShowtimeOccupancyResponseDto>>(body, JsonOptions);

            return apiResponse?.Data?.Items.ToDictionary(x => x.ShowtimeId, x => x.BookedSeats)
                   ?? new Dictionary<Guid, int>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting showtime occupancy from Booking.API");
            return new Dictionary<Guid, int>();
        }
    }
}
