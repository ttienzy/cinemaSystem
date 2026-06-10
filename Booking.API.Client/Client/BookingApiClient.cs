using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Booking.API.Client.Client;

public class BookingApiClient : IBookingApiClient
{
    private const string ApiPrefix = "api";
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public BookingApiClient(HttpClient http) => _http = http;

    public Task<ApiResponse<SeatAvailabilityResponse>> GetSeatAvailabilityAsync(Guid showtimeId, CancellationToken cancellationToken = default)
    {
        return GetAsync<SeatAvailabilityResponse>($"{ApiPrefix}/showtimes/{showtimeId}/seats", cancellationToken);
    }

    public Task<ApiResponse<SeatLockResult>> LockSeatsAsync(Guid showtimeId, LockSeatsRequest request, CancellationToken cancellationToken = default)
    {
        return SendJsonAsync<SeatLockResult>(HttpMethod.Post, $"{ApiPrefix}/showtimes/{showtimeId}/seats/lock", request, cancellationToken);
    }

    public Task<ApiResponse<bool>> UnlockSeatsAsync(Guid showtimeId, UnlockSeatsRequest request, CancellationToken cancellationToken = default)
    {
        return SendJsonAsync<bool>(HttpMethod.Post, $"{ApiPrefix}/showtimes/{showtimeId}/seats/unlock", request, cancellationToken);
    }

    public Task<ApiResponse<BookingResponse>> CreateBookingAsync(CreateBookingRequest request, CancellationToken cancellationToken = default)
    {
        return SendJsonAsync<BookingResponse>(HttpMethod.Post, $"{ApiPrefix}/bookings", request, cancellationToken);
    }

    public Task<ApiResponse<BookingResponse>> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        return GetAsync<BookingResponse>($"{ApiPrefix}/bookings/{bookingId}", cancellationToken);
    }

    public Task<ApiResponse<List<BookingResponse>>> GetUserBookingsAsync(string userId, CancellationToken cancellationToken = default)
    {
        return GetAsync<List<BookingResponse>>($"{ApiPrefix}/bookings/user/{Uri.EscapeDataString(userId)}", cancellationToken);
    }

    public Task<ApiResponse<bool>> CancelBookingAsync(Guid bookingId, CancelBookingRequest request, CancellationToken cancellationToken = default)
    {
        return SendJsonAsync<bool>(HttpMethod.Put, $"{ApiPrefix}/bookings/{bookingId}/cancel", request, cancellationToken);
    }

    public Task<ApiResponse<ShowtimeOccupancyResponse>> GetShowtimeOccupancyAsync(GetShowtimeOccupancyRequest request, CancellationToken cancellationToken = default)
    {
        return SendJsonAsync<ShowtimeOccupancyResponse>(HttpMethod.Post, $"{ApiPrefix}/bookings/analytics/showtime-occupancy", request, cancellationToken);
    }

    public Task<ApiResponse<PaginatedResponse<TicketOperationResponse>>> SearchTicketsAsync(
        string? query = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var queryString = new QueryStringBuilder()
            .Add("q", query)
            .Add("pageNumber", pageNumber.ToString(CultureInfo.InvariantCulture))
            .Add("pageSize", pageSize.ToString(CultureInfo.InvariantCulture))
            .ToString();

        return GetAsync<PaginatedResponse<TicketOperationResponse>>($"{ApiPrefix}/bookings/operations/tickets{queryString}", cancellationToken);
    }

    public Task<ApiResponse<TicketOperationResponse>> CheckInTicketAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        return SendJsonAsync<TicketOperationResponse>(HttpMethod.Put, $"{ApiPrefix}/bookings/operations/tickets/{bookingId}/check-in", new { }, cancellationToken);
    }

    public Task<ApiResponse<DashboardSummaryDto>> GetDashboardSummaryAsync(int utcOffsetMinutes = 420, CancellationToken cancellationToken = default)
    {
        return GetAsync<DashboardSummaryDto>($"{ApiPrefix}/bookings/dashboard/summary?utcOffsetMinutes={utcOffsetMinutes}", cancellationToken);
    }

    public Task<ApiResponse<DashboardKpiSnapshotDto>> GetDashboardKpiSnapshotAsync(int utcOffsetMinutes = 420, CancellationToken cancellationToken = default)
    {
        return GetAsync<DashboardKpiSnapshotDto>($"{ApiPrefix}/bookings/dashboard/kpi-snapshot?utcOffsetMinutes={utcOffsetMinutes}", cancellationToken);
    }

    private Task<ApiResponse<T>> GetAsync<T>(string requestUri, CancellationToken cancellationToken)
    {
        return SendAsync<T>(new HttpRequestMessage(HttpMethod.Get, requestUri), cancellationToken);
    }

    private Task<ApiResponse<T>> SendJsonAsync<T>(HttpMethod method, string requestUri, object value, CancellationToken cancellationToken)
    {
        return SendAsync<T>(new HttpRequestMessage(method, requestUri)
        {
            Content = JsonContent.Create(value)
        }, cancellationToken);
    }

    private async Task<ApiResponse<T>> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using var response = await _http.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(content))
        {
            var body = JsonSerializer.Deserialize<ApiResponse<T>>(content, JsonOptions);
            if (body is not null)
            {
                return body;
            }
        }

        return ApiResponse<T>.FailureResponse($"Booking API returned {(int)response.StatusCode}.", (int)response.StatusCode);
    }

    private sealed class QueryStringBuilder
    {
        private readonly StringBuilder _builder = new();

        public QueryStringBuilder Add(string name, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return this;
            }

            _builder.Append(_builder.Length == 0 ? '?' : '&');
            _builder.Append(Uri.EscapeDataString(name));
            _builder.Append('=');
            _builder.Append(Uri.EscapeDataString(value));
            return this;
        }

        public override string ToString() => _builder.ToString();
    }
}
