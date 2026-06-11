namespace Movie.API.Client.Client;

public interface IMovieApiClient
{
    Task<ApiResponse<PaginatedResponse<MovieDto>>> GetMoviesAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<ApiResponse<MovieSearchResponseDto>> SearchMoviesAsync(string query, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<ApiResponse<MovieDetailDto>> GetMovieByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<PaginatedResponse<MovieAdminListItemDto>>> GetAdminMoviesAsync(string? search = null, string? status = null, Guid? genreId = null, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<ApiResponse<MovieAdminSummaryDto>> GetMovieAdminSummaryAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<MovieEmbeddingRebuildResponseDto>> RebuildMovieEmbeddingsAsync(int limit = 50, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<MovieDto>>> GetMoviesByGenreAsync(Guid genreId, CancellationToken cancellationToken = default);
    Task<ApiResponse<MovieDto>> CreateMovieAsync(CreateMovieRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<MovieDto>> UpdateMovieAsync(Guid id, UpdateMovieRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteMovieAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResponse<List<GenreDto>>> GetGenresAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<GenreDto>> GetGenreByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<GenreDto>> CreateGenreAsync(CreateGenreRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<GenreDto>> UpdateGenreAsync(Guid id, CreateGenreRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteGenreAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResponse<ShowtimeDetailDto>> GetShowtimeByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<ShowtimeDto>>> GetShowtimesByMovieIdAsync(Guid movieId, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<ShowtimeDto>>> GetShowtimesByCinemaHallIdAsync(Guid cinemaHallId, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<ShowtimeDto>>> GetUpcomingShowtimesAsync(int count = 20, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<ShowtimeLookupItemDto>>> GetShowtimesByRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<ShowtimeLookupItemDto>>> LookupShowtimesAsync(ShowtimeLookupRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<ShowtimeDto>> CreateShowtimeAsync(CreateShowtimeRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<ShowtimeDto>> UpdateShowtimeAsync(Guid id, UpdateShowtimeRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteShowtimeAsync(Guid id, CancellationToken cancellationToken = default);
}
