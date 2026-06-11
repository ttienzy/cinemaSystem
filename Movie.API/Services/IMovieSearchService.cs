using Movie.API.Client;

namespace Movie.API.Services;

public interface IMovieSearchService
{
    Task<ApiResponse<MovieSearchResponseDto>> SearchAsync(
        string? query,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
