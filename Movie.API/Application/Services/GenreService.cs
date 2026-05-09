using Cinema.Shared.Models;
using Movie.API.Application.DTOs;
using Movie.API.Domain.Entities;
using Movie.API.Infrastructure.Persistence.Repositories;

namespace Movie.API.Application.Services;

public class GenreService : IGenreService
{
    private readonly IGenreRepository _genreRepository;

    public GenreService(IGenreRepository genreRepository)
    {
        _genreRepository = genreRepository;
    }

    public async Task<ApiResponse<List<GenreDto>>> GetAllAsync()
    {
        var genres = await _genreRepository.GetAllAsync();
        var dtos = genres.Select(MapToDto).ToList();

        return ApiResponse<List<GenreDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<GenreDto>> GetByIdAsync(Guid id)
    {
        var genre = await _genreRepository.GetByIdAsync(id);
        if (genre == null)
        {
            return ApiResponse<GenreDto>.NotFoundResponse("Genre not found");
        }

        var dto = MapToDto(genre);
        return ApiResponse<GenreDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<GenreDto>> CreateAsync(CreateGenreRequest request)
    {
        var genre = new Genre
        {
            Name = request.Name
        };

        var created = await _genreRepository.CreateAsync(genre);
        var dto = MapToDto(created);

        return ApiResponse<GenreDto>.SuccessResponse(dto, "Genre created successfully", 201);
    }

    public async Task<ApiResponse<GenreDto>> UpdateAsync(Guid id, CreateGenreRequest request)
    {
        var existing = await _genreRepository.GetByIdAsync(id);
        if (existing == null)
        {
            return ApiResponse<GenreDto>.NotFoundResponse("Genre not found");
        }

        var genre = new Genre
        {
            Name = request.Name
        };

        var updated = await _genreRepository.UpdateAsync(id, genre);
        var dto = MapToDto(updated!);

        return ApiResponse<GenreDto>.SuccessResponse(dto, "Genre updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        var genre = await _genreRepository.GetByIdAsync(id);
        if (genre == null)
        {
            return ApiResponse<bool>.NotFoundResponse("Genre not found");
        }

        if (genre.MovieGenres.Any())
        {
            return ApiResponse<bool>.FailureResponse(
                "Cannot delete genre with associated movies",
                400,
                new List<ErrorDetail>
                {
                    new ErrorDetail("GENRE_HAS_MOVIES", "This genre is associated with movies", "GenreId")
                }
            );
        }

        var deleted = await _genreRepository.DeleteAsync(id);
        return ApiResponse<bool>.SuccessResponse(deleted, "Genre deleted successfully");
    }

    private GenreDto MapToDto(Genre genre)
    {
        return new GenreDto
        {
            Id = genre.Id,
            Name = genre.Name
        };
    }
}


