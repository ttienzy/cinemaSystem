using Cinema.API.Application.DTOs;
using Cinema.API.Domain.Entities;
using Cinema.API.Infrastructure.Persistence.Repositories;
using Cinema.Shared.Models;
using CinemaEntity = Cinema.API.Domain.Entities.Cinema;

namespace Cinema.API.Application.Services;

public class CinemaService : ICinemaService
{
    private readonly ICinemaRepository _cinemaRepository;

    public CinemaService(ICinemaRepository cinemaRepository)
    {
        _cinemaRepository = cinemaRepository;
    }

    public async Task<ApiResponse<PaginatedResponse<CinemaDto>>> GetAllAsync(int pageNumber, int pageSize)
    {
        var allCinemas = await _cinemaRepository.GetAllAsync();
        var totalCount = allCinemas.Count;

        var cinemas = allCinemas
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToDto)
            .ToList();

        var paginatedResult = PaginatedResponse<CinemaDto>.Create(cinemas, totalCount, pageNumber, pageSize);

        return ApiResponse<PaginatedResponse<CinemaDto>>.SuccessResponse(paginatedResult);
    }

    public async Task<ApiResponse<PaginatedResponse<CinemaAdminOverviewDto>>> GetAdminOverviewAsync(
        string? search,
        string? city,
        string? status,
        int pageNumber,
        int pageSize)
    {
        var allCinemas = await _cinemaRepository.GetAllAsync();

        if (!TryNormalizeCinemaStatus(status, out var normalizedStatus))
        {
            return ApiResponse<PaginatedResponse<CinemaAdminOverviewDto>>.ValidationErrorResponse(
                "Validation failed",
                [
                    new ErrorDetail(
                        "INVALID_CINEMA_STATUS",
                        $"Status must be one of: {string.Join(", ", CinemaStatuses.All)}",
                        "status")
                ]);
        }

        var query = allCinemas.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var trimmedSearch = search.Trim();
            query = query.Where(cinema =>
                cinema.Name.Contains(trimmedSearch, StringComparison.OrdinalIgnoreCase) ||
                cinema.Address.Contains(trimmedSearch, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            var trimmedCity = city.Trim();
            query = query.Where(cinema => (cinema.City ?? string.Empty).Contains(trimmedCity, StringComparison.OrdinalIgnoreCase));
        }

        if (normalizedStatus is not null)
        {
            query = query.Where(cinema => DetermineCinemaStatus(cinema) == normalizedStatus);
        }

        var filteredCinemas = query
            .OrderBy(cinema => cinema.Name)
            .ToList();

        var totalCount = filteredCinemas.Count;
        var items = filteredCinemas
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToAdminOverviewDto)
            .ToList();

        var paginatedResult = PaginatedResponse<CinemaAdminOverviewDto>.Create(items, totalCount, pageNumber, pageSize);
        return ApiResponse<PaginatedResponse<CinemaAdminOverviewDto>>.SuccessResponse(paginatedResult);
    }

    public async Task<ApiResponse<CinemaAdminSummaryDto>> GetAdminSummaryAsync()
    {
        var allCinemas = await _cinemaRepository.GetAllAsync();

        var summary = new CinemaAdminSummaryDto
        {
            TotalCinemas = allCinemas.Count,
            ActiveCinemas = allCinemas.Count(cinema => DetermineCinemaStatus(cinema) == CinemaStatuses.Active),
            InactiveCinemas = allCinemas.Count(cinema => DetermineCinemaStatus(cinema) == CinemaStatuses.Inactive),
            TotalHalls = allCinemas.Sum(cinema => cinema.CinemaHalls.Count),
            TotalSeats = allCinemas.Sum(cinema => cinema.CinemaHalls.Sum(hall => hall.TotalSeats))
        };

        return ApiResponse<CinemaAdminSummaryDto>.SuccessResponse(summary);
    }

    public async Task<ApiResponse<CinemaDetailDto>> GetByIdAsync(Guid id)
    {
        var cinema = await _cinemaRepository.GetByIdAsync(id);
        if (cinema == null)
        {
            return ApiResponse<CinemaDetailDto>.NotFoundResponse("Cinema not found");
        }

        var dto = MapToDetailDto(cinema);
        return ApiResponse<CinemaDetailDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<CinemaDto>> CreateAsync(CreateCinemaRequest request)
    {
        var cinema = new CinemaEntity
        {
            Name = request.Name,
            Address = request.Address,
            City = request.City
        };

        var created = await _cinemaRepository.CreateAsync(cinema);
        var dto = MapToDto(created);

        return ApiResponse<CinemaDto>.SuccessResponse(dto, "Cinema created successfully", 201);
    }

    public async Task<ApiResponse<CinemaDto>> UpdateAsync(Guid id, CreateCinemaRequest request)
    {
        var existing = await _cinemaRepository.GetByIdAsync(id);
        if (existing == null)
        {
            return ApiResponse<CinemaDto>.NotFoundResponse("Cinema not found");
        }

        var cinema = new CinemaEntity
        {
            Name = request.Name,
            Address = request.Address,
            City = request.City
        };

        var updated = await _cinemaRepository.UpdateAsync(id, cinema);
        var dto = MapToDto(updated!);

        return ApiResponse<CinemaDto>.SuccessResponse(dto, "Cinema updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        var cinema = await _cinemaRepository.GetByIdAsync(id);
        if (cinema == null)
        {
            return ApiResponse<bool>.NotFoundResponse("Cinema not found");
        }

        if (cinema.CinemaHalls.Any())
        {
            return ApiResponse<bool>.FailureResponse(
                "Cannot delete cinema with existing halls",
                400,
                new List<ErrorDetail>
                {
                    new ErrorDetail("CINEMA_HAS_HALLS", "This cinema has cinema halls", "CinemaId")
                }
            );
        }

        var deleted = await _cinemaRepository.DeleteAsync(id);
        return ApiResponse<bool>.SuccessResponse(deleted, "Cinema deleted successfully");
    }

    private CinemaDto MapToDto(CinemaEntity cinema)
    {
        return new CinemaDto
        {
            Id = cinema.Id,
            Name = cinema.Name,
            Address = cinema.Address,
            City = cinema.City,
            Status = DetermineCinemaStatus(cinema),
            TotalHalls = cinema.CinemaHalls.Count,
            TotalSeats = cinema.CinemaHalls.Sum(hall => hall.TotalSeats),
            CreatedAt = cinema.CreatedAt
        };
    }

    private CinemaDetailDto MapToDetailDto(CinemaEntity cinema)
    {
        return new CinemaDetailDto
        {
            Id = cinema.Id,
            Name = cinema.Name,
            Address = cinema.Address,
            City = cinema.City,
            Status = DetermineCinemaStatus(cinema),
            TotalHalls = cinema.CinemaHalls.Count,
            TotalSeats = cinema.CinemaHalls.Sum(hall => hall.TotalSeats),
            CreatedAt = cinema.CreatedAt,
            CinemaHalls = cinema.CinemaHalls.Select(MapHallToDto).ToList()
        };
    }

    private CinemaAdminOverviewDto MapToAdminOverviewDto(CinemaEntity cinema)
    {
        return new CinemaAdminOverviewDto
        {
            Id = cinema.Id,
            Name = cinema.Name,
            Address = cinema.Address,
            City = cinema.City,
            Status = DetermineCinemaStatus(cinema),
            TotalHalls = cinema.CinemaHalls.Count,
            TotalSeats = cinema.CinemaHalls.Sum(hall => hall.TotalSeats),
            CreatedAt = cinema.CreatedAt,
            CinemaHalls = cinema.CinemaHalls
                .OrderBy(hall => hall.Name)
                .Select(MapHallToDto)
                .ToList()
        };
    }

    private static CinemaHallDto MapHallToDto(CinemaHall hall)
    {
        var seats = hall.Seats.ToList();

        return new CinemaHallDto
        {
            Id = hall.Id,
            CinemaId = hall.CinemaId,
            Name = hall.Name,
            TotalSeats = hall.TotalSeats,
            CreatedAt = hall.CreatedAt
        };
    }

    private static string DetermineCinemaStatus(CinemaEntity cinema)
    {
        return cinema.CinemaHalls.Any(hall => hall.Seats.Count > 0)
            ? CinemaStatuses.Active
            : CinemaStatuses.Inactive;
    }

    private static bool TryNormalizeCinemaStatus(string? status, out string? normalizedStatus)
    {
        normalizedStatus = null;

        if (string.IsNullOrWhiteSpace(status))
        {
            return true;
        }

        normalizedStatus = status.Trim().ToLowerInvariant() switch
        {
            "active" => CinemaStatuses.Active,
            "inactive" => CinemaStatuses.Inactive,
            _ => null
        };

        return normalizedStatus is not null;
    }
}



