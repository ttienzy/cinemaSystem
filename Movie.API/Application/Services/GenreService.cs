using Cinema.Shared.Models;
using Movie.API.Application.Mappers;
using Movie.API.Domain.Exceptions;


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
        var dtos = genres.Select(x => x.MapToDto()).ToList();

        return ApiResponse<List<GenreDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<GenreDto>> GetByIdAsync(Guid id)
    {
        var genre = await _genreRepository.GetByIdAsync(id);
        if (genre == null)
        {
            return ApiResponse<GenreDto>.NotFoundResponse(GenreException.GENRE_NOT_FOUND);
        }

        var dto = genre.MapToDto();
        return ApiResponse<GenreDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<GenreDto>> CreateAsync(CreateGenreRequest request)
    {
        var genre = new Genre
        {
            Name = request.Name
        };

        var created = await _genreRepository.CreateAsync(genre);
        var dto = created.MapToDto();

        return ApiResponse<GenreDto>.SuccessResponse(dto, GenreException.GENRE_CREATED_SUCCESSFULLY, 201);
    }

    public async Task<ApiResponse<GenreDto>> UpdateAsync(Guid id, CreateGenreRequest request)
    {
        var existing = await _genreRepository.GetByIdAsync(id);
        if (existing == null)
        {
            return ApiResponse<GenreDto>.NotFoundResponse(GenreException.GENRE_NOT_FOUND);
        }

        var genre = new Genre
        {
            Name = request.Name
        };

        var updated = await _genreRepository.UpdateAsync(id, genre);
        var dto = updated.MapToDto();

        return ApiResponse<GenreDto>.SuccessResponse(dto, GenreException.GENRE_UPDATED_SUCCESSFULLY);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        var genre = await _genreRepository.GetByIdAsync(id);
        if (genre == null)
        {
            return ApiResponse<bool>.NotFoundResponse(GenreException.GENRE_NOT_FOUND);
        }

        if (genre.MovieGenres.Any())
        {
            return ApiResponse<bool>.FailureResponse(
                GenreException.GENRE_CANNOT_DELETE_HAS_MOVIES,
                400,
                new List<ErrorDetail>
                {
                    new ErrorDetail(GenreException.GENRE_HAS_MOVIES.Item1, GenreException.GENRE_HAS_MOVIES.Item2, GenreException.GENRE_HAS_MOVIES.Item3)
                }
            );
        }

        var deleted = await _genreRepository.DeleteAsync(id);
        return ApiResponse<bool>.SuccessResponse(deleted, GenreException.GENRE_DELETED_SUCCESSFULLY);
    }

}


