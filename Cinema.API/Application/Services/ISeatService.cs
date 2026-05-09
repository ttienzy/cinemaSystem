using Cinema.API.Application.DTOs;
using Cinema.Shared.Models;

namespace Cinema.API.Application.Services;

public interface ISeatService
{
    Task<ApiResponse<List<SeatDto>>> GetByHallIdAsync(Guid hallId);
    Task<ApiResponse<SeatDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<SeatDto>> CreateAsync(CreateSeatRequest request);
    Task<ApiResponse<List<SeatDto>>> BulkCreateAsync(BulkCreateSeatsRequest request);
    Task<ApiResponse<SeatDto>> UpdateAsync(Guid id, UpdateSeatRequest request);
    Task<ApiResponse<bool>> DeleteAsync(Guid id);
    Task<ApiResponse<bool>> BulkDeleteAsync(List<Guid> seatIds);
}
