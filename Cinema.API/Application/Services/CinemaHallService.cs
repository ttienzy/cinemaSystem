using Cinema.API.Application.DTOs;
using Cinema.API.Domain.Entities;
using Cinema.API.Infrastructure.Persistence.Repositories;
using Cinema.Shared.Models;

namespace Cinema.API.Application.Services;

public class CinemaHallService : ICinemaHallService
{
    private readonly ICinemaHallRepository _hallRepository;
    private readonly ICinemaRepository _cinemaRepository;

    public CinemaHallService(ICinemaHallRepository hallRepository, ICinemaRepository cinemaRepository)
    {
        _hallRepository = hallRepository;
        _cinemaRepository = cinemaRepository;
    }

    public async Task<ApiResponse<List<CinemaHallDto>>> GetByCinemaIdAsync(Guid cinemaId)
    {
        var cinema = await _cinemaRepository.GetByIdAsync(cinemaId);
        if (cinema == null)
        {
            return ApiResponse<List<CinemaHallDto>>.NotFoundResponse("Cinema not found");
        }

        var halls = await _hallRepository.GetByCinemaIdAsync(cinemaId);
        var dtos = halls.Select(MapToDto).ToList();

        return ApiResponse<List<CinemaHallDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<CinemaHallDetailDto>> GetByIdAsync(Guid id)
    {
        var hall = await _hallRepository.GetByIdAsync(id);
        if (hall == null)
        {
            return ApiResponse<CinemaHallDetailDto>.NotFoundResponse("Cinema hall not found");
        }

        var dto = MapToDetailDto(hall);
        return ApiResponse<CinemaHallDetailDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<List<CinemaHallDto>>> GetByIdsAsync(List<Guid> hallIds)
    {
        var normalizedIds = hallIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (normalizedIds.Count == 0)
        {
            return ApiResponse<List<CinemaHallDto>>.ValidationErrorResponse(
                "Validation failed",
                [new ErrorDetail("CINEMA_HALL_IDS_REQUIRED", "At least one cinema hall id is required", "CinemaHallIds")]);
        }

        var halls = await _hallRepository.GetByIdsAsync(normalizedIds);
        var dtos = halls.Select(MapToDto).ToList();

        return ApiResponse<List<CinemaHallDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<List<SeatDto>>> GetSeatsByHallIdAsync(Guid hallId)
    {
        var hall = await _hallRepository.GetByIdAsync(hallId);
        if (hall == null)
        {
            return ApiResponse<List<SeatDto>>.NotFoundResponse("Cinema hall not found");
        }

        var seats = await _hallRepository.GetSeatsByHallIdAsync(hallId);
        var dtos = seats.Select(MapSeatToDto).ToList();

        return ApiResponse<List<SeatDto>>.SuccessResponse(dtos);
    }

    private CinemaHallDto MapToDto(CinemaHall hall)
    {
        var seats = hall.Seats.ToList();

        return new CinemaHallDto
        {
            Id = hall.Id,
            CinemaId = hall.CinemaId,
            Name = hall.Name,
            TotalSeats = hall.TotalSeats,
            SeatMapConfigured = seats.Count > 0,
            CreatedAt = hall.CreatedAt
        };
    }

    private CinemaHallDetailDto MapToDetailDto(CinemaHall hall)
    {
        var hallDto = MapToDto(hall);

        return new CinemaHallDetailDto
        {
            Id = hallDto.Id,
            CinemaId = hallDto.CinemaId,
            Name = hallDto.Name,
            TotalSeats = hallDto.TotalSeats,
            SeatMapConfigured = hallDto.SeatMapConfigured,
            CreatedAt = hallDto.CreatedAt,
            CinemaName = hall.Cinema?.Name ?? string.Empty,
            Seats = hall.Seats.Select(MapSeatToDto).ToList()
        };
    }

    private SeatDto MapSeatToDto(Seat seat)
    {
        return new SeatDto
        {
            Id = seat.Id,
            CinemaHallId = seat.CinemaHallId,
            Row = seat.Row,
            Number = seat.Number,
            DisplayName = $"{seat.Row}{seat.Number}",
        };
    }

    public async Task<ApiResponse<CinemaHallDto>> CreateAsync(CreateCinemaHallRequest request)
    {
        // Validate cinema exists
        var cinema = await _cinemaRepository.GetByIdAsync(request.CinemaId);
        if (cinema == null)
        {
            return ApiResponse<CinemaHallDto>.NotFoundResponse("Cinema not found");
        }

        var hall = new CinemaHall
        {
            CinemaId = request.CinemaId,
            Name = request.Name,
            TotalSeats = 0
        };

        await _hallRepository.AddAsync(hall);
        await _hallRepository.SaveChangesAsync();

        var dto = MapToDto(hall);
        return ApiResponse<CinemaHallDto>.SuccessResponse(dto, "Cinema hall created successfully");
    }

    public async Task<ApiResponse<CinemaHallDto>> UpdateAsync(Guid id, UpdateCinemaHallRequest request)
    {
        var hall = await _hallRepository.GetByIdAsync(id);
        if (hall == null)
        {
            return ApiResponse<CinemaHallDto>.NotFoundResponse("Cinema hall not found");
        }

        hall.Name = request.Name;
        hall.UpdatedAt = DateTime.UtcNow;

        _hallRepository.Update(hall);
        await _hallRepository.SaveChangesAsync();

        var dto = MapToDto(hall);
        return ApiResponse<CinemaHallDto>.SuccessResponse(dto, "Cinema hall updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        var hall = await _hallRepository.GetByIdAsync(id);
        if (hall == null)
        {
            return ApiResponse<bool>.NotFoundResponse("Cinema hall not found");
        }

        // Check if hall has seats
        var seats = await _hallRepository.GetSeatsByHallIdAsync(id);
        if (seats.Any())
        {
            return ApiResponse<bool>.ValidationErrorResponse(
                "Cannot delete cinema hall with existing seats",
                [new ErrorDetail("HALL_HAS_SEATS", "Please delete all seats before deleting the hall", "CinemaHallId")]);
        }

        _hallRepository.Delete(hall);
        await _hallRepository.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "Cinema hall deleted successfully");
    }

}


