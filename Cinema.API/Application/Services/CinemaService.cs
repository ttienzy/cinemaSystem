using Cinema.API.Application.DTOs;
using Cinema.API.Application.Mappers;
using Cinema.API.Domain.Entities;
using Cinema.API.Domain.Exceptions;
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
            .Select(cinema => cinema.CinemaMapToDto())
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

        if (!CinemaEntity.TryNormalizeStatus(status, out var normalizedStatus))
        {
            var value = CinemaException.INVALID_CINEMA_STATUS(string.Join(", ", CinemaStatuses.All));
            return ApiResponse<PaginatedResponse<CinemaAdminOverviewDto>>.ValidationErrorResponse(
                CinemaException.VALIDATION_FAILED,
                [new ErrorDetail(value.Item1, value.Item2, value.Item3)]);
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
            query = query.Where(cinema => cinema.MatchesStatus(normalizedStatus));
        }

        var filteredCinemas = query
            .OrderBy(cinema => cinema.Name)
            .ToList();

        var totalCount = filteredCinemas.Count;
        var items = filteredCinemas
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(cinema => cinema.CinemaMapToAdminOverviewDto())
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
            ActiveCinemas = allCinemas.Count(cinema => cinema.MatchesStatus(CinemaStatuses.Active)),
            InactiveCinemas = allCinemas.Count(cinema => cinema.MatchesStatus(CinemaStatuses.Inactive)),
            TotalHalls = allCinemas.Sum(cinema => cinema.GetTotalHalls()),
            TotalSeats = allCinemas.Sum(cinema => cinema.GetTotalSeats())
        };

        return ApiResponse<CinemaAdminSummaryDto>.SuccessResponse(summary);
    }

    public async Task<ApiResponse<CinemaDetailDto>> GetByIdAsync(Guid id)
    {
        var cinema = await _cinemaRepository.GetByIdAsync(id);
        if (cinema == null)
        {
            return ApiResponse<CinemaDetailDto>.NotFoundResponse(CinemaException.CINEMA_NOT_FOUND);
        }

        var dto = cinema.CinemaMapToDetailDto();
        return ApiResponse<CinemaDetailDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<CinemaDto>> CreateAsync(CreateCinemaRequest request)
    {
        var cinema = CinemaEntity.Create(request.Name, request.Address, request.City);

        var created = await _cinemaRepository.CreateAsync(cinema);
        var dto = created.CinemaMapToDto();

        return ApiResponse<CinemaDto>.SuccessResponse(dto, CinemaException.CINEMA_CREATED_SUCCESSFULLY, 201);
    }

    public async Task<ApiResponse<CinemaDto>> UpdateAsync(Guid id, CreateCinemaRequest request)
    {
        var existing = await _cinemaRepository.GetByIdAsync(id);
        if (existing == null)
        {
            return ApiResponse<CinemaDto>.NotFoundResponse(CinemaException.CINEMA_NOT_FOUND);
        }

        var cinema = new CinemaEntity();
        cinema.UpdateDetails(request.Name, request.Address, request.City);

        var updated = await _cinemaRepository.UpdateAsync(id, cinema);
        var dto = updated!.CinemaMapToDto();

        return ApiResponse<CinemaDto>.SuccessResponse(dto, CinemaException.CINEMA_UPDATED_SUCCESSFULLY);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        var cinema = await _cinemaRepository.GetByIdAsync(id);
        if (cinema == null)
        {
            return ApiResponse<bool>.NotFoundResponse(CinemaException.CINEMA_NOT_FOUND);
        }

        if (cinema.HasCinemaHalls())
        {
            var value = CinemaException.CINEMA_HAS_HALLS;
            return ApiResponse<bool>.FailureResponse(
                CinemaException.CANNOT_DELETE_CINEMA_HAS_HALLS,
                400,
                [new ErrorDetail(value.Item1, value.Item2, value.Item3)]);
        }

        var deleted = await _cinemaRepository.DeleteAsync(id);
        return ApiResponse<bool>.SuccessResponse(deleted, CinemaException.CINEMA_DELETED_SUCCESSFULLY);
    }
}



