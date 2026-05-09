using Cinema.API.Application.DTOs;
using Cinema.Shared.Models;

namespace Cinema.API.Application.Services;

public interface ICinemaService
{
    Task<ApiResponse<PaginatedResponse<CinemaDto>>> GetAllAsync(int pageNumber, int pageSize);
    Task<ApiResponse<PaginatedResponse<CinemaAdminOverviewDto>>> GetAdminOverviewAsync(
        string? search,
        string? city,
        string? status,
        int pageNumber,
        int pageSize);
    Task<ApiResponse<CinemaAdminSummaryDto>> GetAdminSummaryAsync();
    Task<ApiResponse<CinemaDetailDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<CinemaDto>> CreateAsync(CreateCinemaRequest request);
    Task<ApiResponse<CinemaDto>> UpdateAsync(Guid id, CreateCinemaRequest request);
    Task<ApiResponse<bool>> DeleteAsync(Guid id);
}


