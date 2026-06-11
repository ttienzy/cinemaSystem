using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Cinema.API.Client.Client;

public class CinemaApiClient : ICinemaApiClient
{
    private const string ApiPrefix = "api";
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CinemaApiClient(HttpClient http) => _http = http;

    public Task<ApiResponse<PaginatedResponse<CinemaDto>>> GetCinemasAsync(
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return GetAsync<PaginatedResponse<CinemaDto>>(
            $"{ApiPrefix}/cinemas?pageNumber={pageNumber}&pageSize={pageSize}",
            cancellationToken);
    }

    public Task<ApiResponse<CinemaDetailDto>> GetCinemaByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return GetAsync<CinemaDetailDto>($"{ApiPrefix}/cinemas/{id}", cancellationToken);
    }

    public Task<ApiResponse<PaginatedResponse<CinemaAdminOverviewDto>>> GetCinemaAdminOverviewAsync(
        string? search = null,
        string? city = null,
        string? status = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new QueryStringBuilder()
            .Add("pageNumber", pageNumber.ToString())
            .Add("pageSize", pageSize.ToString())
            .Add("search", search)
            .Add("city", city)
            .Add("status", status)
            .ToString();

        return GetAsync<PaginatedResponse<CinemaAdminOverviewDto>>(
            $"{ApiPrefix}/cinemas/admin/overview{query}",
            cancellationToken);
    }

    public Task<ApiResponse<CinemaAdminSummaryDto>> GetCinemaAdminSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        return GetAsync<CinemaAdminSummaryDto>($"{ApiPrefix}/cinemas/admin/summary", cancellationToken);
    }

    public Task<ApiResponse<CinemaDto>> CreateCinemaAsync(
        CreateCinemaRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendJsonAsync<CinemaDto>(HttpMethod.Post, $"{ApiPrefix}/cinemas", request, cancellationToken);
    }

    public Task<ApiResponse<CinemaDto>> UpdateCinemaAsync(
        Guid id,
        CreateCinemaRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendJsonAsync<CinemaDto>(HttpMethod.Put, $"{ApiPrefix}/cinemas/{id}", request, cancellationToken);
    }

    public Task<ApiResponse<bool>> DeleteCinemaAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return DeleteAsync($"{ApiPrefix}/cinemas/{id}", cancellationToken);
    }

    public Task<ApiResponse<List<CinemaHallDto>>> GetHallsByCinemaIdAsync(
        Guid cinemaId,
        CancellationToken cancellationToken = default)
    {
        return GetAsync<List<CinemaHallDto>>($"{ApiPrefix}/cinema-halls/cinema/{cinemaId}", cancellationToken);
    }

    public Task<ApiResponse<CinemaHallDetailDto>> GetHallByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return GetAsync<CinemaHallDetailDto>($"{ApiPrefix}/cinema-halls/{id}", cancellationToken);
    }

    public Task<ApiResponse<List<CinemaHallDto>>> LookupHallsAsync(
        CinemaHallLookupRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendJsonAsync<List<CinemaHallDto>>(
            HttpMethod.Post,
            $"{ApiPrefix}/cinema-halls/lookup",
            request,
            cancellationToken);
    }

    public Task<ApiResponse<List<SeatDto>>> GetHallSeatsAsync(
        Guid hallId,
        CancellationToken cancellationToken = default)
    {
        return GetAsync<List<SeatDto>>($"{ApiPrefix}/cinema-halls/{hallId}/seats", cancellationToken);
    }

    public Task<ApiResponse<CinemaHallDto>> CreateHallAsync(
        CreateCinemaHallRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendJsonAsync<CinemaHallDto>(HttpMethod.Post, $"{ApiPrefix}/cinema-halls", request, cancellationToken);
    }

    public Task<ApiResponse<CinemaHallDto>> UpdateHallAsync(
        Guid id,
        UpdateCinemaHallRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendJsonAsync<CinemaHallDto>(HttpMethod.Put, $"{ApiPrefix}/cinema-halls/{id}", request, cancellationToken);
    }

    public Task<ApiResponse<bool>> DeleteHallAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return DeleteAsync($"{ApiPrefix}/cinema-halls/{id}", cancellationToken);
    }

    public Task<ApiResponse<List<SeatDto>>> GetSeatsByHallIdAsync(
        Guid hallId,
        CancellationToken cancellationToken = default)
    {
        return GetAsync<List<SeatDto>>($"{ApiPrefix}/seats/hall/{hallId}", cancellationToken);
    }

    public Task<ApiResponse<SeatDto>> GetSeatByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return GetAsync<SeatDto>($"{ApiPrefix}/seats/{id}", cancellationToken);
    }

    public Task<ApiResponse<SeatDto>> CreateSeatAsync(
        CreateSeatRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendJsonAsync<SeatDto>(HttpMethod.Post, $"{ApiPrefix}/seats", request, cancellationToken);
    }

    public Task<ApiResponse<List<SeatDto>>> BulkCreateSeatsAsync(
        BulkCreateSeatsRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendJsonAsync<List<SeatDto>>(HttpMethod.Post, $"{ApiPrefix}/seats/bulk", request, cancellationToken);
    }

    public Task<ApiResponse<SeatDto>> UpdateSeatAsync(
        Guid id,
        UpdateSeatRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendJsonAsync<SeatDto>(HttpMethod.Put, $"{ApiPrefix}/seats/{id}", request, cancellationToken);
    }

    public Task<ApiResponse<bool>> DeleteSeatAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return DeleteAsync($"{ApiPrefix}/seats/{id}", cancellationToken);
    }

    public Task<ApiResponse<bool>> BulkDeleteSeatsAsync(
        IReadOnlyCollection<Guid> seatIds,
        CancellationToken cancellationToken = default)
    {
        return SendJsonAsync<bool>(HttpMethod.Post, $"{ApiPrefix}/seats/bulk-delete", seatIds, cancellationToken);
    }

    private Task<ApiResponse<T>> GetAsync<T>(string requestUri, CancellationToken cancellationToken)
    {
        return SendAsync<T>(new HttpRequestMessage(HttpMethod.Get, requestUri), cancellationToken);
    }

    private Task<ApiResponse<bool>> DeleteAsync(string requestUri, CancellationToken cancellationToken)
    {
        return SendAsync<bool>(new HttpRequestMessage(HttpMethod.Delete, requestUri), cancellationToken);
    }

    private Task<ApiResponse<T>> SendJsonAsync<T>(
        HttpMethod method,
        string requestUri,
        object value,
        CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(method, requestUri)
        {
            Content = JsonContent.Create(value)
        };

        return SendAsync<T>(request, cancellationToken);
    }

    private async Task<ApiResponse<T>> SendAsync<T>(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
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

        return ApiResponse<T>.FailureResponse(
            $"Cinema API returned {(int)response.StatusCode}.",
            (int)response.StatusCode);
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
