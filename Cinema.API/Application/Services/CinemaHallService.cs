using Cinema.API.Application.DTOs;
using Cinema.API.Application.Mappers;
using Cinema.API.Domain.Entities;
using Cinema.API.Domain.Exceptions;
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
            return ApiResponse<List<CinemaHallDto>>.NotFoundResponse(CinemaException.CINEMA_NOT_FOUND);
        }

        var halls = await _hallRepository.GetByCinemaIdAsync(cinemaId);
        var dtos = halls.Select(hall => hall.CinemaHallMapToDto()).ToList();

        return ApiResponse<List<CinemaHallDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<CinemaHallDetailDto>> GetByIdAsync(Guid id)
    {
        var hall = await _hallRepository.GetByIdAsync(id);
        if (hall == null)
        {
            return ApiResponse<CinemaHallDetailDto>.NotFoundResponse(CinemaHallException.CINEMA_HALL_NOT_FOUND);
        }

        var dto = hall.CinemaHallMapToDetailDto();
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
            var value = CinemaHallException.CINEMA_HALL_IDS_REQUIRED;
            return ApiResponse<List<CinemaHallDto>>.ValidationErrorResponse(
                CinemaHallException.VALIDATION_FAILED,
                [new ErrorDetail(value.Item1, value.Item2, value.Item3)]);
        }

        var halls = await _hallRepository.GetByIdsAsync(normalizedIds);
        var dtos = halls.Select(hall => hall.CinemaHallMapToDto()).ToList();

        return ApiResponse<List<CinemaHallDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<List<SeatDto>>> GetSeatsByHallIdAsync(Guid hallId)
    {
        var hall = await _hallRepository.GetByIdAsync(hallId);
        if (hall == null)
        {
            return ApiResponse<List<SeatDto>>.NotFoundResponse(CinemaHallException.CINEMA_HALL_NOT_FOUND);
        }

        var seats = await _hallRepository.GetSeatsByHallIdAsync(hallId);
        var dtos = seats.Select(seat => seat.SeatMapToDto()).ToList();

        return ApiResponse<List<SeatDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<CinemaHallDto>> CreateAsync(CreateCinemaHallRequest request)
    {
        var cinema = await _cinemaRepository.GetByIdAsync(request.CinemaId);
        if (cinema == null)
        {
            return ApiResponse<CinemaHallDto>.NotFoundResponse(CinemaException.CINEMA_NOT_FOUND);
        }

        var hall = CinemaHall.Create(request.CinemaId, request.Name);

        await _hallRepository.AddAsync(hall);
        await _hallRepository.SaveChangesAsync();

        var dto = hall.CinemaHallMapToDto();
        return ApiResponse<CinemaHallDto>.SuccessResponse(dto, CinemaHallException.CINEMA_HALL_CREATED_SUCCESSFULLY);
    }

    public async Task<ApiResponse<CinemaHallDto>> UpdateAsync(Guid id, UpdateCinemaHallRequest request)
    {
        var hall = await _hallRepository.GetByIdAsync(id);
        if (hall == null)
        {
            return ApiResponse<CinemaHallDto>.NotFoundResponse(CinemaHallException.CINEMA_HALL_NOT_FOUND);
        }

        hall.UpdateName(request.Name);

        _hallRepository.Update(hall);
        await _hallRepository.SaveChangesAsync();

        var dto = hall.CinemaHallMapToDto();
        return ApiResponse<CinemaHallDto>.SuccessResponse(dto, CinemaHallException.CINEMA_HALL_UPDATED_SUCCESSFULLY);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        var hall = await _hallRepository.GetByIdAsync(id);
        if (hall == null)
        {
            return ApiResponse<bool>.NotFoundResponse(CinemaHallException.CINEMA_HALL_NOT_FOUND);
        }

        if (hall.HasSeats())
        {
            var value = CinemaHallException.HALL_HAS_SEATS;
            return ApiResponse<bool>.ValidationErrorResponse(
                CinemaHallException.CANNOT_DELETE_CINEMA_HALL_WITH_SEATS,
                [new ErrorDetail(value.Item1, value.Item2, value.Item3)]);
        }

        _hallRepository.Delete(hall);
        await _hallRepository.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, CinemaHallException.CINEMA_HALL_DELETED_SUCCESSFULLY);
    }

}

