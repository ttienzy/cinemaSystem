using Cinema.API.Application.DTOs;
using Cinema.API.Domain.Entities;
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
            return ApiResponse<List<SeatDto>>.NotFoundResponse("Cinema hall not found");
        }

        var seats = await _seatRepository.GetByHallIdAsync(hallId);
        var dtos = seats.Select(MapToDto).ToList();

        return ApiResponse<List<SeatDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<SeatDto>> GetByIdAsync(Guid id)
    {
        var seat = await _seatRepository.GetByIdAsync(id);
        if (seat == null)
        {
            return ApiResponse<SeatDto>.NotFoundResponse("Seat not found");
        }

        var dto = MapToDto(seat);
        return ApiResponse<SeatDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<SeatDto>> CreateAsync(CreateSeatRequest request)
    {
        // Validate hall exists
        var hall = await _hallRepository.GetByIdAsync(request.CinemaHallId);
        if (hall == null)
        {
            return ApiResponse<SeatDto>.NotFoundResponse("Cinema hall not found");
        }

        // Check if seat already exists
        var existingSeat = await _seatRepository.GetByRowAndNumberAsync(request.CinemaHallId, request.Row, request.Number);
        if (existingSeat != null)
        {
            return ApiResponse<SeatDto>.ValidationErrorResponse(
                "Seat already exists",
                [new ErrorDetail("SEAT_EXISTS", $"Seat {request.Row}{request.Number} already exists in this hall", "Row,Number")]);
        }

        var seat = new Seat
        {
            CinemaHallId = request.CinemaHallId,
            Row = request.Row,
            Number = request.Number,
            CreatedAt = DateTime.UtcNow,
        };

        await _seatRepository.AddAsync(seat);

        // Update hall total seats
        hall.TotalSeats++;
        _hallRepository.Update(hall);

        await _seatRepository.SaveChangesAsync();

        var dto = MapToDto(seat);
        return ApiResponse<SeatDto>.SuccessResponse(dto, "Seat created successfully");
    }

    public async Task<ApiResponse<List<SeatDto>>> BulkCreateAsync(BulkCreateSeatsRequest request)
    {
        // Validate hall exists
        var hall = await _hallRepository.GetByIdAsync(request.CinemaHallId);
        if (hall == null)
        {
            return ApiResponse<List<SeatDto>>.NotFoundResponse("Cinema hall not found");
        }

        // Get existing seats
        var existingSeats = await _seatRepository.GetByHallIdAsync(request.CinemaHallId);
        var existingKeys = existingSeats.Select(s => $"{s.Row}{s.Number}").ToHashSet();

        var seatsToCreate = new List<Seat>();
        var errors = new List<ErrorDetail>();

        foreach (var seatInput in request.Seats)
        {
            var key = $"{seatInput.Row}{seatInput.Number}";
            if (existingKeys.Contains(key))
            {
                errors.Add(new ErrorDetail("SEAT_EXISTS", $"Seat {key} already exists", "Seats"));
                continue;
            }

            if (seatsToCreate.Any(s => s.Row == seatInput.Row && s.Number == seatInput.Number))
            {
                errors.Add(new ErrorDetail("DUPLICATE_SEAT", $"Duplicate seat {key} in request", "Seats"));
                continue;
            }

            seatsToCreate.Add(new Seat
            {
                CinemaHallId = request.CinemaHallId,
                Row = seatInput.Row,
                Number = seatInput.Number,
               
            });
        }

        if (errors.Any())
        {
            return ApiResponse<List<SeatDto>>.ValidationErrorResponse("Some seats could not be created", errors);
        }

        if (!seatsToCreate.Any())
        {
            return ApiResponse<List<SeatDto>>.ValidationErrorResponse(
                "No seats to create",
                [new ErrorDetail("NO_SEATS", "All seats already exist", "Seats")]);
        }

        await _seatRepository.AddRangeAsync(seatsToCreate);

        // Update hall total seats
        hall.TotalSeats += seatsToCreate.Count;
        _hallRepository.Update(hall);

        await _seatRepository.SaveChangesAsync();

        var dtos = seatsToCreate.Select(MapToDto).ToList();
        return ApiResponse<List<SeatDto>>.SuccessResponse(dtos, $"{seatsToCreate.Count} seats created successfully");
    }

    public async Task<ApiResponse<SeatDto>> UpdateAsync(Guid id, UpdateSeatRequest request)
    {
        var seat = await _seatRepository.GetByIdAsync(id);
        if (seat == null)
        {
            return ApiResponse<SeatDto>.NotFoundResponse("Seat not found");
        }

        // Check if new position conflicts with existing seat
        if (seat.Row != request.Row || seat.Number != request.Number)
        {
            var existingSeat = await _seatRepository.GetByRowAndNumberAsync(seat.CinemaHallId, request.Row, request.Number);
            if (existingSeat != null && existingSeat.Id != id)
            {
                return ApiResponse<SeatDto>.ValidationErrorResponse(
                    "Seat position already occupied",
                    [new ErrorDetail("SEAT_EXISTS", $"Seat {request.Row}{request.Number} already exists", "Row,Number")]);
            }
        }

        seat.Row = request.Row;
        seat.Number = request.Number;
        seat.UpdatedAt = DateTime.UtcNow;

        _seatRepository.Update(seat);
        await _seatRepository.SaveChangesAsync();

        var dto = MapToDto(seat);
        return ApiResponse<SeatDto>.SuccessResponse(dto, "Seat updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        var seat = await _seatRepository.GetByIdAsync(id);
        if (seat == null)
        {
            return ApiResponse<bool>.NotFoundResponse("Seat not found");
        }

        var hall = await _hallRepository.GetByIdAsync(seat.CinemaHallId);

        _seatRepository.Delete(seat);

        // Update hall total seats
        if (hall != null)
        {
            hall.TotalSeats = Math.Max(0, hall.TotalSeats - 1);
            _hallRepository.Update(hall);
        }

        await _seatRepository.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "Seat deleted successfully");
    }

    public async Task<ApiResponse<bool>> BulkDeleteAsync(List<Guid> seatIds)
    {
        if (!seatIds.Any())
        {
            return ApiResponse<bool>.ValidationErrorResponse(
                "No seats to delete",
                [new ErrorDetail("NO_SEATS", "Seat IDs list is empty", "SeatIds")]);
        }

        var seats = await _seatRepository.GetByIdsAsync(seatIds);
        if (!seats.Any())
        {
            return ApiResponse<bool>.NotFoundResponse("No seats found");
        }

        var hallIds = seats.Select(s => s.CinemaHallId).Distinct().ToList();

        _seatRepository.DeleteRange(seats);

        // Update hall total seats for each affected hall
        foreach (var hallId in hallIds)
        {
            var hall = await _hallRepository.GetByIdAsync(hallId);
            if (hall != null)
            {
                var deletedCount = seats.Count(s => s.CinemaHallId == hallId);
                hall.TotalSeats = Math.Max(0, hall.TotalSeats - deletedCount);
                _hallRepository.Update(hall);
            }
        }

        await _seatRepository.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, $"{seats.Count} seats deleted successfully");
    }

    private SeatDto MapToDto(Seat seat)
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
}
