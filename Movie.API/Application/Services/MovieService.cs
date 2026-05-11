using Cinema.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Movie.API.Application.DTOs;
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

        var totalCount = allMovies.Count;

        var movies = allMovies
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToDto)
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
            query = query.Where(movie => DetermineMovieStatus(movie, now) == normalizedStatus);
        }

        if (genreId.HasValue)
        {
            query = query.Where(movie => movie.MovieGenres.Any(movieGenre => movieGenre.GenreId == genreId.Value));
        }

        var filteredMovies = query
            .OrderBy(movie => GetMovieStatusRank(DetermineMovieStatus(movie, now)))
            .ThenByDescending(movie => movie.ReleaseDate)
            .ToList();

        var totalCount = filteredMovies.Count;
        var movies = filteredMovies
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(movie => MapToAdminListItemDto(movie, now))
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
            ShowingMovies = allMovies.Count(movie => DetermineMovieStatus(movie, now) == MovieStatuses.Showing),
            ComingSoonMovies = allMovies.Count(movie => DetermineMovieStatus(movie, now) == MovieStatuses.ComingSoon),
            ArchivedMovies = allMovies.Count(movie => DetermineMovieStatus(movie, now) == MovieStatuses.Archived)
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

        var dto = MapToDetailDto(movie);
        return ApiResponse<MovieDetailDto>.SuccessResponse(dto);
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

        var movie = new MovieEntity
        {
            Title = request.Title,
            Description = request.Description,
            Duration = request.Duration,
            Language = request.Language,
            ReleaseDate = request.ReleaseDate,
            PosterUrl = posterUrlResult.Data,
            MovieGenres = normalizedGenreIds.Select(gId => new MovieGenre
            {
                GenreId = gId
            }).ToList()
        };

        var created = await _movieRepository.CreateAsync(movie);
        var dto = MapToDto(created);

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

        var movie = new MovieEntity
        {
            Title = request.Title,
            Description = request.Description,
            Duration = request.Duration,
            Language = request.Language,
            ReleaseDate = request.ReleaseDate,
            PosterUrl = posterUrl
        };

        var updated = await _movieRepository.UpdateAsync(id, movie, normalizedGenreIds);
        var dto = MapToDto(updated!);

        return ApiResponse<MovieDto>.SuccessResponse(dto, MovieException.MOVIE_UPDATED_SUCCESSFULLY);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        var movie = await _movieRepository.GetByIdAsync(id);
        if (movie == null)
        {
            return ApiResponse<bool>.NotFoundResponse(MovieException.MOVIE_NOT_FOUND);
        }

        if (movie.Showtimes.Any())
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

    public async Task<ApiResponse<List<MovieDto>>> GetByGenreAsync(Guid genreId)
    {
        var genre = await _genreRepository.GetByIdAsync(genreId);
        if (genre == null)
        {
            return ApiResponse<List<MovieDto>>.NotFoundResponse(GenreException.GENRE_NOT_FOUND);
        }

        var movies = await _movieRepository.GetByGenreAsync(genreId);
        var dtos = movies.Select(MapToDto).ToList();

        return ApiResponse<List<MovieDto>>.SuccessResponse(dtos);
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

    private MovieDto MapToDto(MovieEntity movie)
    {
        var now = DateTime.UtcNow;

        return new MovieDto
        {
            Id = movie.Id,
            Title = movie.Title,
            Description = movie.Description,
            Duration = movie.Duration,
            Language = movie.Language,
            ReleaseDate = movie.ReleaseDate,
            PosterUrl = movie.PosterUrl,
            Status = DetermineMovieStatus(movie, now),
            CreatedAt = movie.CreatedAt,
            Genres = movie.MovieGenres
                .Where(mg => mg.Genre is not null)
                .Select(mg => new GenreDto
                {
                    Id = mg.Genre!.Id,
                    Name = mg.Genre.Name
                })
                .ToList()
        };
    }

    private MovieDetailDto MapToDetailDto(MovieEntity movie)
    {
        var now = DateTime.UtcNow;

        return new MovieDetailDto
        {
            Id = movie.Id,
            Title = movie.Title,
            Description = movie.Description,
            Duration = movie.Duration,
            Language = movie.Language,
            ReleaseDate = movie.ReleaseDate,
            PosterUrl = movie.PosterUrl,
            Status = DetermineMovieStatus(movie, now),
            CreatedAt = movie.CreatedAt,
            Genres = movie.MovieGenres
                .Where(mg => mg.Genre is not null)
                .Select(mg => new GenreDto
                {
                    Id = mg.Genre!.Id,
                    Name = mg.Genre.Name
                })
                .ToList(),
            Showtimes = movie.Showtimes.Select(s => new ShowtimeDto
            {
                Id = s.Id,
                MovieId = s.MovieId,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                Price = s.Price,
                CinemaHallId = s.CinemaHallId,
                CreatedAt = s.CreatedAt,
                MovieTitle = movie.Title,
                DurationMinutes = movie.Duration,
                
            }).ToList()
        };
    }

    private MovieAdminListItemDto MapToAdminListItemDto(MovieEntity movie, DateTime now)
    {
        var upcomingShowtimes = movie.Showtimes
            .Where(showtime => showtime.EndTime >= now)
            .OrderBy(showtime => showtime.StartTime)
            .ToList();

        var lastShowtime = movie.Showtimes
            .OrderByDescending(showtime => showtime.EndTime)
            .FirstOrDefault();

        return new MovieAdminListItemDto
        {
            Id = movie.Id,
            Title = movie.Title,
            Description = movie.Description,
            Duration = movie.Duration,
            Language = movie.Language,
            ReleaseDate = movie.ReleaseDate,
            PosterUrl = movie.PosterUrl,
            Status = DetermineMovieStatus(movie, now),
            CreatedAt = movie.CreatedAt,
            Genres = movie.MovieGenres
                .Where(mg => mg.Genre is not null)
                .Select(mg => new GenreDto
                {
                    Id = mg.Genre!.Id,
                    Name = mg.Genre.Name
                })
                .ToList(),
            TotalShowtimes = movie.Showtimes.Count,
            UpcomingShowtimesCount = upcomingShowtimes.Count,
            NextShowtimeAt = upcomingShowtimes.FirstOrDefault()?.StartTime,
            LastShowtimeAt = lastShowtime?.EndTime
        };
    }

    private static string DetermineMovieStatus(MovieEntity movie, DateTime now)
    {
        if (movie.ReleaseDate.Date > now.Date)
        {
            return MovieStatuses.ComingSoon;
        }

        if (movie.Showtimes.Any(showtime => showtime.EndTime >= now))
        {
            return MovieStatuses.Showing;
        }

        return MovieStatuses.Archived;
    }

    private static int GetMovieStatusRank(string status)
    {
        return status switch
        {
            MovieStatuses.Showing => 0,
            MovieStatuses.ComingSoon => 1,
            MovieStatuses.Archived => 2,
            _ => 3
        };
    }

    
}



