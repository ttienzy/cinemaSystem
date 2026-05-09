using Cinema.Shared.Models;
using Movie.API.Application.DTOs;

namespace Movie.API.Application.Services;

public interface IGenreService
{
    Task<ApiResponse<List<GenreDto>>> GetAllAsync();
    Task<ApiResponse<GenreDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<GenreDto>> CreateAsync(CreateGenreRequest request);
    Task<ApiResponse<GenreDto>> UpdateAsync(Guid id, CreateGenreRequest request);
    Task<ApiResponse<bool>> DeleteAsync(Guid id);
}


