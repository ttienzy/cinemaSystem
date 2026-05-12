using Cinema.API.Application.DTOs;
using Cinema.API.Application.Mappers;
using Cinema.API.Domain.Entities;
using Cinema.API.Domain.Exceptions;
using Cinema.API.Infrastructure.Persistence.Repositories;
using Cinema.Shared.Models;

namespace Cinema.API.Application.Services;

public class SeatService : ISeatService
{
    private readonly ISeatRepository _seatRepository;
    private readonly ICinemaHallRepository _hallRepository;

    public SeatService(ISeatRepository seatRepository, ICinemaHallRepository hallRepository)
    {
        _seatRepository = seatRepository;
        _hallRepository = hallRepository;
    }

    public async Task<ApiResponse<List<SeatDto>>> GetByHallIdAsync(Guid hallId)
    {
        var hall = await _hallRepository.GetByIdAsync(hallId);
        if (hall == null)
        {
            return ApiResponse<List<SeatDto>>.NotFoundResponse(CinemaHallException.CINEMA_HALL_NOT_FOUND);
        }

        var seats = await _seatRepository.GetByHallIdAsync(hallId);
        var dtos = seats.Select(seat => seat.SeatMapToDto()).ToList();

        return ApiResponse<List<SeatDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<SeatDto>> GetByIdAsync(Guid id)
    {
        var seat = await _seatRepository.GetByIdAsync(id);
        if (seat == null)
        {
            return ApiResponse<SeatDto>.NotFoundResponse(SeatException.SEAT_NOT_FOUND);
        }

        var dto = seat.SeatMapToDto();
        return ApiResponse<SeatDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<SeatDto>> CreateAsync(CreateSeatRequest request)
    {
        var hall = await _hallRepository.GetByIdAsync(request.CinemaHallId);
        if (hall == null)
        {
            return ApiResponse<SeatDto>.NotFoundResponse(CinemaHallException.CINEMA_HALL_NOT_FOUND);
        }

        var existingSeat = await _seatRepository.GetByRowAndNumberAsync(request.CinemaHallId, request.Row, request.Number);
        if (existingSeat != null)
        {
            var value = SeatException.SEAT_EXISTS($"{request.Row}{request.Number}");
            return ApiResponse<SeatDto>.ValidationErrorResponse(
                SeatException.SEAT_ALREADY_EXISTS,
                [new ErrorDetail(value.Item1, value.Item2, value.Item3)]);
        }

        var seat = Seat.Create(request.CinemaHallId, request.Row, request.Number);

        await _seatRepository.AddAsync(seat);

        hall.IncreaseTotalSeats();
        _hallRepository.Update(hall);

        await _seatRepository.SaveChangesAsync();

        var dto = seat.SeatMapToDto();
        return ApiResponse<SeatDto>.SuccessResponse(dto, SeatException.SEAT_CREATED_SUCCESSFULLY);
    }

    public async Task<ApiResponse<List<SeatDto>>> BulkCreateAsync(BulkCreateSeatsRequest request)
    {
        var hall = await _hallRepository.GetByIdAsync(request.CinemaHallId);
        if (hall == null)
        {
            return ApiResponse<List<SeatDto>>.NotFoundResponse(CinemaHallException.CINEMA_HALL_NOT_FOUND);
        }

        var existingSeats = await _seatRepository.GetByHallIdAsync(request.CinemaHallId);
        var existingKeys = existingSeats.Select(seat => seat.GetDisplayName()).ToHashSet();

        var seatsToCreate = new List<Seat>();
        var errors = new List<ErrorDetail>();

        foreach (var seatInput in request.Seats)
        {
            var key = $"{seatInput.Row}{seatInput.Number}";
            if (existingKeys.Contains(key))
            {
                var value = SeatException.SEAT_EXISTS(key);
                errors.Add(new ErrorDetail(value.Item1, value.Item2, "Seats"));
                continue;
            }

            if (seatsToCreate.Any(seat => seat.MatchesPosition(seatInput.Row, seatInput.Number)))
            {
                var value = SeatException.DUPLICATE_SEAT(key);
                errors.Add(new ErrorDetail(value.Item1, value.Item2, value.Item3));
                continue;
            }

            seatsToCreate.Add(Seat.Create(request.CinemaHallId, seatInput.Row, seatInput.Number));
        }

        if (errors.Any())
        {
            return ApiResponse<List<SeatDto>>.ValidationErrorResponse(SeatException.SOME_SEATS_COULD_NOT_BE_CREATED, errors);
        }

        if (!seatsToCreate.Any())
        {
            var value = SeatException.NO_SEATS_TO_CREATE;
            return ApiResponse<List<SeatDto>>.ValidationErrorResponse(
                SeatException.NO_SEATS_TO_CREATE_MESSAGE,
                [new ErrorDetail(value.Item1, value.Item2, value.Item3)]);
        }

        await _seatRepository.AddRangeAsync(seatsToCreate);

        hall.IncreaseTotalSeats(seatsToCreate.Count);
        _hallRepository.Update(hall);

        await _seatRepository.SaveChangesAsync();

        var dtos = seatsToCreate.Select(seat => seat.SeatMapToDto()).ToList();
        return ApiResponse<List<SeatDto>>.SuccessResponse(dtos, $"{seatsToCreate.Count} seats created successfully");
    }

    public async Task<ApiResponse<SeatDto>> UpdateAsync(Guid id, UpdateSeatRequest request)
    {
        var seat = await _seatRepository.GetByIdAsync(id);
        if (seat == null)
        {
            return ApiResponse<SeatDto>.NotFoundResponse(SeatException.SEAT_NOT_FOUND);
        }

        if (!seat.MatchesPosition(request.Row, request.Number))
        {
            var existingSeat = await _seatRepository.GetByRowAndNumberAsync(seat.CinemaHallId, request.Row, request.Number);
            if (existingSeat != null && existingSeat.Id != id)
            {
                var value = SeatException.SEAT_EXISTS($"{request.Row}{request.Number}");
                return ApiResponse<SeatDto>.ValidationErrorResponse(
                    SeatException.SEAT_POSITION_ALREADY_OCCUPIED,
                    [new ErrorDetail(value.Item1, value.Item2, value.Item3)]);
            }
        }

        seat.UpdatePosition(request.Row, request.Number);

        _seatRepository.Update(seat);
        await _seatRepository.SaveChangesAsync();

        var dto = seat.SeatMapToDto();
        return ApiResponse<SeatDto>.SuccessResponse(dto, SeatException.SEAT_UPDATED_SUCCESSFULLY);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        var seat = await _seatRepository.GetByIdAsync(id);
        if (seat == null)
        {
            return ApiResponse<bool>.NotFoundResponse(SeatException.SEAT_NOT_FOUND);
        }

        var hall = await _hallRepository.GetByIdAsync(seat.CinemaHallId);

        _seatRepository.Delete(seat);

        if (hall != null)
        {
            hall.DecreaseTotalSeats();
            _hallRepository.Update(hall);
        }

        await _seatRepository.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, SeatException.SEAT_DELETED_SUCCESSFULLY);
    }

    public async Task<ApiResponse<bool>> BulkDeleteAsync(List<Guid> seatIds)
    {
        if (!seatIds.Any())
        {
            var value = SeatException.NO_SEAT_IDS;
            return ApiResponse<bool>.ValidationErrorResponse(
                SeatException.NO_SEATS_TO_DELETE,
                [new ErrorDetail(value.Item1, value.Item2, value.Item3)]);
        }

        var seats = await _seatRepository.GetByIdsAsync(seatIds);
        if (!seats.Any())
        {
            return ApiResponse<bool>.NotFoundResponse(SeatException.NO_SEATS_FOUND);
        }

        var hallIds = seats.Select(s => s.CinemaHallId).Distinct().ToList();

        _seatRepository.DeleteRange(seats);

        foreach (var hallId in hallIds)
        {
            var hall = await _hallRepository.GetByIdAsync(hallId);
            if (hall != null)
            {
                var deletedCount = seats.Count(s => s.CinemaHallId == hallId);
                hall.DecreaseTotalSeats(deletedCount);
                _hallRepository.Update(hall);
            }
        }

        await _seatRepository.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, $"{seats.Count} seats deleted successfully");
    }
}
