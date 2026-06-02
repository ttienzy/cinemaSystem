using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Movie.API.Client.Client;

public class MovieApiClient : IMovieApiClient
{
    private const string ApiPrefix = "api/v1";
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public MovieApiClient(HttpClient http) => _http = http;

    public Task<ApiResponse<PaginatedResponse<MovieDto>>> GetMoviesAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return GetAsync<PaginatedResponse<MovieDto>>($"{ApiPrefix}/movies?pageNumber={pageNumber}&pageSize={pageSize}", cancellationToken);
    }

    public Task<ApiResponse<MovieDetailDto>> GetMovieByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return GetAsync<MovieDetailDto>($"{ApiPrefix}/movies/{id}", cancellationToken);
    }

    public Task<ApiResponse<PaginatedResponse<MovieAdminListItemDto>>> GetAdminMoviesAsync(
        string? search = null,
        string? status = null,
        Guid? genreId = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new QueryStringBuilder()
            .Add("pageNumber", pageNumber.ToString(CultureInfo.InvariantCulture))
            .Add("pageSize", pageSize.ToString(CultureInfo.InvariantCulture))
            .Add("search", search)
            .Add("status", status)
            .Add("genreId", genreId?.ToString())
            .ToString();

        return GetAsync<PaginatedResponse<MovieAdminListItemDto>>($"{ApiPrefix}/movies/admin/list{query}", cancellationToken);
    }

    public Task<ApiResponse<MovieAdminSummaryDto>> GetMovieAdminSummaryAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<MovieAdminSummaryDto>($"{ApiPrefix}/movies/admin/summary", cancellationToken);
    }

    public Task<ApiResponse<List<MovieDto>>> GetMoviesByGenreAsync(Guid genreId, CancellationToken cancellationToken = default)
    {
        return GetAsync<List<MovieDto>>($"{ApiPrefix}/movies/genre/{genreId}", cancellationToken);
    }

    public Task<ApiResponse<MovieDto>> CreateMovieAsync(CreateMovieRequest request, CancellationToken cancellationToken = default)
    {
        return SendAsync<MovieDto>(new HttpRequestMessage(HttpMethod.Post, $"{ApiPrefix}/movies")
        {
            Content = CreateMovieMultipart(request)
        }, cancellationToken);
    }

    public Task<ApiResponse<MovieDto>> UpdateMovieAsync(Guid id, UpdateMovieRequest request, CancellationToken cancellationToken = default)
    {
        return SendAsync<MovieDto>(new HttpRequestMessage(HttpMethod.Put, $"{ApiPrefix}/movies/{id}")
        {
            Content = CreateMovieMultipart(request)
        }, cancellationToken);
    }

    public Task<ApiResponse<bool>> DeleteMovieAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return DeleteAsync($"{ApiPrefix}/movies/{id}", cancellationToken);
    }

    public Task<ApiResponse<List<GenreDto>>> GetGenresAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<List<GenreDto>>($"{ApiPrefix}/genres", cancellationToken);
    }

    public Task<ApiResponse<GenreDto>> GetGenreByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return GetAsync<GenreDto>($"{ApiPrefix}/genres/{id}", cancellationToken);
    }

    public Task<ApiResponse<GenreDto>> CreateGenreAsync(CreateGenreRequest request, CancellationToken cancellationToken = default)
    {
        return SendJsonAsync<GenreDto>(HttpMethod.Post, $"{ApiPrefix}/genres", request, cancellationToken);
    }

    public Task<ApiResponse<GenreDto>> UpdateGenreAsync(Guid id, CreateGenreRequest request, CancellationToken cancellationToken = default)
    {
        return SendJsonAsync<GenreDto>(HttpMethod.Put, $"{ApiPrefix}/genres/{id}", request, cancellationToken);
    }

    public Task<ApiResponse<bool>> DeleteGenreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return DeleteAsync($"{ApiPrefix}/genres/{id}", cancellationToken);
    }

    public Task<ApiResponse<ShowtimeDetailDto>> GetShowtimeByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return GetAsync<ShowtimeDetailDto>($"{ApiPrefix}/showtimes/{id}", cancellationToken);
    }

    public Task<ApiResponse<List<ShowtimeDto>>> GetShowtimesByMovieIdAsync(Guid movieId, CancellationToken cancellationToken = default)
    {
        return GetAsync<List<ShowtimeDto>>($"{ApiPrefix}/showtimes/movie/{movieId}", cancellationToken);
    }

    public Task<ApiResponse<List<ShowtimeDto>>> GetShowtimesByCinemaHallIdAsync(Guid cinemaHallId, CancellationToken cancellationToken = default)
    {
        return GetAsync<List<ShowtimeDto>>($"{ApiPrefix}/showtimes/cinemahall/{cinemaHallId}", cancellationToken);
    }

    public Task<ApiResponse<List<ShowtimeDto>>> GetUpcomingShowtimesAsync(int count = 20, CancellationToken cancellationToken = default)
    {
        return GetAsync<List<ShowtimeDto>>($"{ApiPrefix}/showtimes/upcoming?count={count}", cancellationToken);
    }

    public Task<ApiResponse<List<ShowtimeLookupItemDto>>> GetShowtimesByRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var query = new QueryStringBuilder()
            .Add("from", from.ToString("O", CultureInfo.InvariantCulture))
            .Add("to", to.ToString("O", CultureInfo.InvariantCulture))
            .ToString();

        return GetAsync<List<ShowtimeLookupItemDto>>($"{ApiPrefix}/showtimes/range{query}", cancellationToken);
    }

    public Task<ApiResponse<List<ShowtimeLookupItemDto>>> LookupShowtimesAsync(ShowtimeLookupRequest request, CancellationToken cancellationToken = default)
    {
        return SendJsonAsync<List<ShowtimeLookupItemDto>>(HttpMethod.Post, $"{ApiPrefix}/showtimes/lookup", request, cancellationToken);
    }

    public Task<ApiResponse<ShowtimeDto>> CreateShowtimeAsync(CreateShowtimeRequest request, CancellationToken cancellationToken = default)
    {
        return SendJsonAsync<ShowtimeDto>(HttpMethod.Post, $"{ApiPrefix}/showtimes", request, cancellationToken);
    }

    public Task<ApiResponse<ShowtimeDto>> UpdateShowtimeAsync(Guid id, UpdateShowtimeRequest request, CancellationToken cancellationToken = default)
    {
        return SendJsonAsync<ShowtimeDto>(HttpMethod.Put, $"{ApiPrefix}/showtimes/{id}", request, cancellationToken);
    }

    public Task<ApiResponse<bool>> DeleteShowtimeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return DeleteAsync($"{ApiPrefix}/showtimes/{id}", cancellationToken);
    }

    private Task<ApiResponse<T>> GetAsync<T>(string requestUri, CancellationToken cancellationToken)
    {
        return SendAsync<T>(new HttpRequestMessage(HttpMethod.Get, requestUri), cancellationToken);
    }

    private Task<ApiResponse<bool>> DeleteAsync(string requestUri, CancellationToken cancellationToken)
    {
        return SendAsync<bool>(new HttpRequestMessage(HttpMethod.Delete, requestUri), cancellationToken);
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

        return ApiResponse<T>.FailureResponse($"Movie API returned {(int)response.StatusCode}.", (int)response.StatusCode);
    }

    private static MultipartFormDataContent CreateMovieMultipart(CreateMovieRequest request)
    {
        return CreateMovieMultipartCore(
            request.Title,
            request.Description,
            request.Duration,
            request.Language,
            request.ReleaseDate,
            request.PosterFile,
            request.GenreIds);
    }

    private static MultipartFormDataContent CreateMovieMultipart(UpdateMovieRequest request)
    {
        var content = CreateMovieMultipartCore(
            request.Title,
            request.Description,
            request.Duration,
            request.Language,
            request.ReleaseDate,
            request.PosterFile,
            request.GenreIds);

        content.Add(new StringContent(request.RemovePoster.ToString()), nameof(UpdateMovieRequest.RemovePoster));
        return content;
    }

    private static MultipartFormDataContent CreateMovieMultipartCore(
        string title,
        string? description,
        int duration,
        string? language,
        DateTime releaseDate,
        IFormFile? posterFile,
        IEnumerable<Guid> genreIds)
    {
        var content = new MultipartFormDataContent
        {
            { new StringContent(title), nameof(CreateMovieRequest.Title) },
            { new StringContent(duration.ToString(CultureInfo.InvariantCulture)), nameof(CreateMovieRequest.Duration) },
            { new StringContent(releaseDate.ToString("O", CultureInfo.InvariantCulture)), nameof(CreateMovieRequest.ReleaseDate) }
        };

        AddIfNotNull(content, nameof(CreateMovieRequest.Description), description);
        AddIfNotNull(content, nameof(CreateMovieRequest.Language), language);

        foreach (var genreId in genreIds)
        {
            content.Add(new StringContent(genreId.ToString()), nameof(CreateMovieRequest.GenreIds));
        }

        AddFile(content, nameof(CreateMovieRequest.PosterFile), posterFile);
        return content;
    }

    private static void AddIfNotNull(MultipartFormDataContent content, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            content.Add(new StringContent(value), name);
        }
    }

    private static void AddFile(MultipartFormDataContent content, string name, IFormFile? file)
    {
        if (file is null)
        {
            return;
        }

        var streamContent = new StreamContent(file.OpenReadStream());
        if (!string.IsNullOrWhiteSpace(file.ContentType))
        {
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
        }

        content.Add(streamContent, name, file.FileName);
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
