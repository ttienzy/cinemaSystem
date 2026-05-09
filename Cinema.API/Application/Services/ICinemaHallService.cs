using Cinema.API.Application.DTOs;
using Cinema.Shared.Models;

namespace Cinema.API.Application.Services;

public interface ICinemaHallService
{
    Task<ApiResponse<List<CinemaHallDto>>> GetByCinemaIdAsync(Guid cinemaId);
    Task<ApiResponse<CinemaHallDetailDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<List<SeatDto>>> GetSeatsByHallIdAsync(Guid hallId);
    Task<ApiResponse<List<CinemaHallDto>>> GetByIdsAsync(List<Guid> hallIds);

    // CRUD operations
    Task<ApiResponse<CinemaHallDto>> CreateAsync(CreateCinemaHallRequest request);
    Task<ApiResponse<CinemaHallDto>> UpdateAsync(Guid id, UpdateCinemaHallRequest request);
    Task<ApiResponse<bool>> DeleteAsync(Guid id);
}


