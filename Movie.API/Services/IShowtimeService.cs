using Movie.API.Client;

namespace Movie.API.Services;

public interface IShowtimeService
{
    Task<ApiResponse<ShowtimeDetailDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<List<ShowtimeDto>>> GetByMovieIdAsync(Guid movieId);
    Task<ApiResponse<List<ShowtimeDto>>> GetByCinemaHallIdAsync(Guid cinemaHallId);
    Task<ApiResponse<List<ShowtimeLookupItemDto>>> GetLookupByIdsAsync(List<Guid> showtimeIds);
    Task<ApiResponse<List<ShowtimeLookupItemDto>>> GetByRangeAsync(DateTime from, DateTime to);
    Task<ApiResponse<ShowtimeDto>> CreateAsync(CreateShowtimeRequest request);
    Task<ApiResponse<ShowtimeDto>> UpdateAsync(Guid id, UpdateShowtimeRequest request);
    Task<ApiResponse<bool>> DeleteAsync(Guid id);
    Task<ApiResponse<List<ShowtimeDto>>> GetUpcomingShowtimesAsync(int count = 20);
}


