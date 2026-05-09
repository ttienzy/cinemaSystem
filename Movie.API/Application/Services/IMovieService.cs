using Cinema.Shared.Models;
using Movie.API.Application.DTOs;

namespace Movie.API.Application.Services;

public interface IMovieService
{
    Task<ApiResponse<PaginatedResponse<MovieDto>>> GetAllAsync(int pageNumber, int pageSize);
    Task<ApiResponse<PaginatedResponse<MovieAdminListItemDto>>> GetAdminListAsync(
        string? search,
        string? status,
        Guid? genreId,
        int pageNumber,
        int pageSize);
    Task<ApiResponse<MovieAdminSummaryDto>> GetAdminSummaryAsync();
    Task<ApiResponse<MovieDetailDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<MovieDto>> CreateAsync(CreateMovieRequest request);
    Task<ApiResponse<MovieDto>> UpdateAsync(Guid id, UpdateMovieRequest request);
    Task<ApiResponse<bool>> DeleteAsync(Guid id);
    Task<ApiResponse<List<MovieDto>>> GetByGenreAsync(Guid genreId);
}


