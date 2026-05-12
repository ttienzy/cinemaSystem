using Cinema.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Movie.API.Application.DTOs;
using Movie.API.Application.Mappers;
using Movie.API.Domain.Entities;
using Movie.API.Domain.Exceptions;
using Movie.API.Infrastructure.Persistence.Repositories;
using MovieEntity = Movie.API.Domain.Entities.Movie;


namespace Movie.API.Application.Services;

public class MovieService : IMovieService
{
    private readonly IMovieRepository _movieRepository;
    private readonly IGenreRepository _genreRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<MovieService> _logger;

    public MovieService(
        IMovieRepository movieRepository,
        IGenreRepository genreRepository,
        IFileStorageService fileStorageService,
        ILogger<MovieService> logger)
    {
        _movieRepository = movieRepository;
        _genreRepository = genreRepository;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<ApiResponse<PaginatedResponse<MovieDto>>> GetAllAsync(int pageNumber, int pageSize)
    {
        var allMovies = await _movieRepository.GetAllAsync();
        var now = DateTime.UtcNow;

        var totalCount = allMovies.Count;

        var movies = allMovies
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(movie => movie.MovieMapToDto(now))
            .ToList();

        var paginatedResult = PaginatedResponse<MovieDto>.Create(movies, totalCount, pageNumber, pageSize);

        return ApiResponse<PaginatedResponse<MovieDto>>.SuccessResponse(paginatedResult);
    }

    public async Task<ApiResponse<PaginatedResponse<MovieAdminListItemDto>>> GetAdminListAsync(
        string? search,
        string? status,
        Guid? genreId,
        int pageNumber,
        int pageSize)
    {
        var allMovies = await _movieRepository.GetAllAsync();
        var now = DateTime.UtcNow;

        if (!MovieEntity.TryNormalizeMovieStatus(status, out var normalizedStatus))
        {
            var value = MovieException.MOVIE_INVALID_STATUS(string.Join(",", MovieStatuses.All));
            return ApiResponse<PaginatedResponse<MovieAdminListItemDto>>.ValidationErrorResponse(
                MovieException.VALIDATION_FAILED,
                [new ErrorDetail(value.Item1, value.Item2, value.Item3)]);
        }

        var query = allMovies.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var trimmedSearch = search.Trim();
            query = query.Where(movie => movie.Title.Contains(trimmedSearch, StringComparison.OrdinalIgnoreCase));
        }

        if (normalizedStatus is not null)
        {
            query = query.Where(movie => movie.MatchesStatus(normalizedStatus, now));
        }

        if (genreId.HasValue)
        {
            query = query.Where(movie => movie.HasGenre(genreId.Value));
        }

        var filteredMovies = query
            .OrderBy(movie => movie.GetStatusRank(now))
            .ThenByDescending(movie => movie.ReleaseDate)
            .ToList();

        var totalCount = filteredMovies.Count;
        var movies = filteredMovies
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(movie => movie.MovieMapToAdminListItemDto(now))
            .ToList();

        var paginatedResult = PaginatedResponse<MovieAdminListItemDto>.Create(movies, totalCount, pageNumber, pageSize);
        return ApiResponse<PaginatedResponse<MovieAdminListItemDto>>.SuccessResponse(paginatedResult);
    }

    public async Task<ApiResponse<MovieAdminSummaryDto>> GetAdminSummaryAsync()
    {
        var allMovies = await _movieRepository.GetAllAsync();
        var now = DateTime.UtcNow;

        var summary = new MovieAdminSummaryDto
        {
            TotalMovies = allMovies.Count,
            ShowingMovies = allMovies.Count(movie => movie.MatchesStatus(MovieStatuses.Showing, now)),
            ComingSoonMovies = allMovies.Count(movie => movie.MatchesStatus(MovieStatuses.ComingSoon, now)),
            ArchivedMovies = allMovies.Count(movie => movie.MatchesStatus(MovieStatuses.Archived, now))
        };

        return ApiResponse<MovieAdminSummaryDto>.SuccessResponse(summary);
    }

    public async Task<ApiResponse<MovieDetailDto>> GetByIdAsync(Guid id)
    {
        var movie = await _movieRepository.GetByIdAsync(id);
        if (movie == null)
        {
            return ApiResponse<MovieDetailDto>.NotFoundResponse(MovieException.MOVIE_NOT_FOUND);
        }

        var dto = movie.MovieMapToDetailDto(DateTime.UtcNow);
        return ApiResponse<MovieDetailDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<List<MovieDto>>> GetByGenreAsync(Guid genreId)
    {
        var genre = await _genreRepository.GetByIdAsync(genreId);
        if (genre == null)
        {
            return ApiResponse<List<MovieDto>>.NotFoundResponse(GenreException.GENRE_NOT_FOUND);
        }

        var movies = await _movieRepository.GetByGenreAsync(genreId);
        var now = DateTime.UtcNow;
        var dtos = movies.Select(movie => movie.MovieMapToDto(now)).ToList();

        return ApiResponse<List<MovieDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<MovieDto>> CreateAsync(CreateMovieRequest request)
    {
        var normalizedGenreIds = (request.GenreIds ?? new List<Guid>())
            .Distinct()
            .ToList();

        var validationResult = await ValidateUpsertRequest(normalizedGenreIds, request.ReleaseDate);
        if (!validationResult.Success)
        {
            return ApiResponse<MovieDto>.FailureResponse(
                validationResult.Message,
                400,
                validationResult.Errors
            );
        }

        var posterUrlResult = await UploadPosterIfProvidedAsync(request.PosterFile);
        if (!posterUrlResult.Success)
        {
            return ApiResponse<MovieDto>.FailureResponse(
                posterUrlResult.Message,
                posterUrlResult.StatusCode,
                posterUrlResult.Errors
            );
        }

        var movie = MovieEntity.Create(
            request.Title,
            request.Description,
            request.Duration,
            request.Language,
            request.ReleaseDate,
            posterUrlResult.Data,
            normalizedGenreIds);

        var created = await _movieRepository.CreateAsync(movie);
        var dto = created.MovieMapToDto(DateTime.UtcNow);

        return ApiResponse<MovieDto>.SuccessResponse(dto, MovieException.MOVIE_CREATED_SUCCESSFULLY, 201);
    }

    public async Task<ApiResponse<MovieDto>> UpdateAsync(Guid id, UpdateMovieRequest request)
    {
        var existing = await _movieRepository.GetByIdAsync(id);
        if (existing == null)
        {
            return ApiResponse<MovieDto>.NotFoundResponse(MovieException.MOVIE_NOT_FOUND);
        }

        var normalizedGenreIds = (request.GenreIds ?? new List<Guid>())
            .Distinct()
            .ToList();

        var validationResult = await ValidateUpsertRequest(normalizedGenreIds, request.ReleaseDate);
        if (!validationResult.Success)
        {
            return ApiResponse<MovieDto>.FailureResponse(
                validationResult.Message,
                validationResult.StatusCode,
                validationResult.Errors
            );
        }

        var posterUrl = existing.PosterUrl;
        if (request.RemovePoster)
        {
            posterUrl = null;
        }

        if (request.PosterFile is not null)
        {
            var posterUrlResult = await UploadPosterIfProvidedAsync(request.PosterFile);
            if (!posterUrlResult.Success)
            {
                return ApiResponse<MovieDto>.FailureResponse(
                    posterUrlResult.Message,
                    posterUrlResult.StatusCode,
                    posterUrlResult.Errors
                );
            }

            posterUrl = posterUrlResult.Data;
        }

        var movie = new MovieEntity();
        movie.UpdateDetails(
            request.Title,
            request.Description,
            request.Duration,
            request.Language,
            request.ReleaseDate,
            posterUrl);

        var updated = await _movieRepository.UpdateAsync(id, movie, normalizedGenreIds);
        var dto = updated!.MovieMapToDto(DateTime.UtcNow);

        return ApiResponse<MovieDto>.SuccessResponse(dto, MovieException.MOVIE_UPDATED_SUCCESSFULLY);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        var movie = await _movieRepository.GetByIdAsync(id);
        if (movie == null)
        {
            return ApiResponse<bool>.NotFoundResponse(MovieException.MOVIE_NOT_FOUND);
        }

        if (movie.HasShowtimes())
        {
            var value = MovieException.MOVIE_HAS_SHOWTIMES;
            return ApiResponse<bool>.FailureResponse(
                value.Item1,
                400,
                [new ErrorDetail(value.Item1, value.Item2, value.Item3)]);
        }

        var deleted = await _movieRepository.DeleteAsync(id);
        return ApiResponse<bool>.SuccessResponse(deleted, MovieException.MOVIE_DELETED_SUCCESSFULLY);
    }

    private async Task<ApiResponse<bool>> ValidateUpsertRequest(IEnumerable<Guid> genreIds, DateTime releaseDate)
    {
        var errors = new List<ErrorDetail>();

        foreach (var genreId in genreIds)
        {
            var genre = await _genreRepository.GetByIdAsync(genreId);
            if (genre == null)
            {
                var value = GenreException.GENRE_INVALID_ID(genreId.ToString());
                errors.Add(new ErrorDetail(value.Item1, value.Item2, value.Item3));
            }
        }

        if (releaseDate > DateTime.UtcNow.AddYears(2))
        {
            var value = GenreException.GENRE_INVALID_RELEASE_DATE();
            errors.Add(new ErrorDetail(value.Item1, value.Item2, value.Item3));
        }

        if (errors.Any())
        {
            return ApiResponse<bool>.ValidationErrorResponse(MovieException.VALIDATION_FAILED, errors);
        }

        return ApiResponse<bool>.SuccessResponse(true);
    }

    private async Task<ApiResponse<string?>> UploadPosterIfProvidedAsync(IFormFile? posterFile)
    {
        if (posterFile is null)
        {
            return ApiResponse<string?>.SuccessResponse(null);
        }

        try
        {
            var uploadResult = await _fileStorageService.UploadMoviePosterAsync(posterFile);
            return ApiResponse<string?>.SuccessResponse(uploadResult.Url);
        }
        catch (ArgumentException exception)
        {
            var value = MovieException.INVALID_POSTER_FILE(exception.Message);
            return ApiResponse<string?>.FailureResponse(
                value.Item1,
                400,
                [new ErrorDetail(value.Item1, value.Item2, value.Item3)]
            );
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Poster upload failed for file {FileName}", posterFile.FileName);

            var value = MovieException.POSTER_UPLOAD_FAILED;
            return ApiResponse<string?>.FailureResponse(
                value.Item1,
                502,
                [new ErrorDetail(value.Item1, value.Item2, value.Item3)]
            );
        }
    }
}

