using Cinema.Shared.Models;
using Microsoft.Extensions.Options;
using Movie.API.Application.DTOs;
using Movie.API.Domain.Entities;
using Movie.API.Infrastructure.Persistence.Repositories;

namespace Movie.API.Application.Services;

public class ShowtimeService : IShowtimeService
{
    private readonly IShowtimeRepository _showtimeRepository;
    private readonly IMovieRepository _movieRepository;
    private readonly ICinemaApiClient _cinemaApiClient;
    private readonly IBookingApiClient _bookingApiClient;
    private readonly SchedulingOptions _schedulingOptions;
    private readonly ILogger<ShowtimeService> _logger;

    public ShowtimeService(
        IShowtimeRepository showtimeRepository,
        IMovieRepository movieRepository,
        ICinemaApiClient cinemaApiClient,
        IBookingApiClient bookingApiClient,
        IOptions<SchedulingOptions> schedulingOptions,
        ILogger<ShowtimeService> logger)
    {
        _showtimeRepository = showtimeRepository;
        _movieRepository = movieRepository;
        _cinemaApiClient = cinemaApiClient;
        _bookingApiClient = bookingApiClient;
        _schedulingOptions = schedulingOptions.Value;
        _logger = logger;
    }

    public async Task<ApiResponse<ShowtimeDetailDto>> GetByIdAsync(Guid id)
    {
        var showtime = await _showtimeRepository.GetByIdAsync(id);
        if (showtime == null)
        {
            return ApiResponse<ShowtimeDetailDto>.NotFoundResponse("Showtime not found");
        }

        var cinemaHallInfo = await _cinemaApiClient.GetCinemaHallInfoAsync(showtime.CinemaHallId);
        var dto = await MapToDetailDtoAsync(showtime, cinemaHallInfo);

        return ApiResponse<ShowtimeDetailDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<List<ShowtimeDto>>> GetByMovieIdAsync(Guid movieId)
    {
        var movie = await _movieRepository.GetByIdAsync(movieId);
        if (movie == null)
        {
            return ApiResponse<List<ShowtimeDto>>.NotFoundResponse("Movie not found");
        }

        var showtimes = await _showtimeRepository.GetByMovieIdAsync(movieId);
        var dtos = new List<ShowtimeDto>();

        foreach (var showtime in showtimes)
        {
            var cinemaHallInfo = await _cinemaApiClient.GetCinemaHallInfoAsync(showtime.CinemaHallId);
            dtos.Add(MapToDto(showtime, cinemaHallInfo));
        }

        return ApiResponse<List<ShowtimeDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<List<ShowtimeDto>>> GetByCinemaHallIdAsync(Guid cinemaHallId)
    {
        var cinemaHallExists = await _cinemaApiClient.ValidateCinemaHallExistsAsync(cinemaHallId);
        if (!cinemaHallExists)
        {
            return ApiResponse<List<ShowtimeDto>>.NotFoundResponse("Cinema hall not found");
        }

        var showtimes = await _showtimeRepository.GetByCinemaHallIdAsync(cinemaHallId);
        var cinemaHallInfo = await _cinemaApiClient.GetCinemaHallInfoAsync(cinemaHallId);
        var dtos = showtimes.Select(s => MapToDto(s, cinemaHallInfo)).ToList();

        return ApiResponse<List<ShowtimeDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<ShowtimeDto>> CreateAsync(CreateShowtimeRequest request)
    {
        var validationResult = await ValidateCreateRequestAsync(request);
        if (!validationResult.Success)
        {
            return ApiResponse<ShowtimeDto>.FailureResponse(
                validationResult.Message,
                validationResult.StatusCode,
                validationResult.Errors
            );
        }

        var movie = await _movieRepository.GetByIdAsync(request.MovieId);
        if (movie == null)
        {
            return ApiResponse<ShowtimeDto>.NotFoundResponse("Movie not found");
        }

        var endTime = request.StartTime.AddMinutes(movie.Duration);

        var hasOverlap = await _showtimeRepository.HasOverlappingShowtimeAsync(
            request.CinemaHallId,
            request.StartTime,
            endTime.AddMinutes(_schedulingOptions.CleaningBufferMinutes),
            _schedulingOptions.CleaningBufferMinutes
        );

        if (hasOverlap)
        {
            return ApiResponse<ShowtimeDto>.FailureResponse(
                "Showtime overlaps with existing showtime in this cinema hall",
                400,
                new List<ErrorDetail>
                {
                    new ErrorDetail(
                        "SHOWTIME_OVERLAP",
                        "The selected time slot conflicts with another showtime",
                        "StartTime"
                    )
                }
            );
        }

        var showtime = new Showtime
        {
            MovieId = request.MovieId,
            CinemaHallId = request.CinemaHallId,
            StartTime = request.StartTime,
            EndTime = endTime,
            Price = request.Price
        };

        var created = await _showtimeRepository.CreateAsync(showtime);
        var cinemaHallInfo = await _cinemaApiClient.GetCinemaHallInfoAsync(request.CinemaHallId);
        var dto = MapToDto(created, cinemaHallInfo);

        _logger.LogInformation(
            "Showtime created: {ShowtimeId} for Movie {MovieId} at Hall {CinemaHallId}",
            created.Id, request.MovieId, request.CinemaHallId
        );

        return ApiResponse<ShowtimeDto>.SuccessResponse(dto, "Showtime created successfully", 201);
    }

    public async Task<ApiResponse<ShowtimeDto>> UpdateAsync(Guid id, UpdateShowtimeRequest request)
    {
        var existing = await _showtimeRepository.GetByIdAsync(id);
        if (existing == null)
        {
            return ApiResponse<ShowtimeDto>.NotFoundResponse("Showtime not found");
        }

        var bookedSeats = await GetBookedSeatsAsync(existing.Id);
        if (bookedSeats > 0)
        {
            return ApiResponse<ShowtimeDto>.FailureResponse(
                "Cannot reschedule a showtime that already has sold tickets",
                400,
                [new ErrorDetail("SHOWTIME_HAS_BOOKINGS", "This showtime already has sold tickets", "Id")]);
        }

        if (existing.StartTime < DateTime.UtcNow)
        {
            return ApiResponse<ShowtimeDto>.FailureResponse(
                "Cannot update past showtimes",
                400,
                new List<ErrorDetail>
                {
                    new ErrorDetail("PAST_SHOWTIME", "This showtime has already started or passed", "Id")
                }
            );
        }

        var movie = await _movieRepository.GetByIdAsync(existing.MovieId);
        var endTime = request.StartTime.AddMinutes(movie!.Duration);

        var hasOverlap = await _showtimeRepository.HasOverlappingShowtimeAsync(
            existing.CinemaHallId,
            request.StartTime,
            endTime.AddMinutes(_schedulingOptions.CleaningBufferMinutes),
            _schedulingOptions.CleaningBufferMinutes,
            id
        );

        if (hasOverlap)
        {
            return ApiResponse<ShowtimeDto>.FailureResponse(
                "Showtime overlaps with existing showtime",
                400,
                new List<ErrorDetail>
                {
                    new ErrorDetail("SHOWTIME_OVERLAP", "Time slot conflicts with another showtime", "StartTime")
                }
            );
        }

        var showtime = new Showtime
        {
            StartTime = request.StartTime,
            EndTime = endTime,
            Price = request.Price
        };

        var updated = await _showtimeRepository.UpdateAsync(id, showtime);
        var cinemaHallInfo = await _cinemaApiClient.GetCinemaHallInfoAsync(existing.CinemaHallId);
        var dto = MapToDto(updated!, cinemaHallInfo);

        return ApiResponse<ShowtimeDto>.SuccessResponse(dto, "Showtime updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        var showtime = await _showtimeRepository.GetByIdAsync(id);
        if (showtime == null)
        {
            return ApiResponse<bool>.NotFoundResponse("Showtime not found");
        }

        var bookedSeats = await GetBookedSeatsAsync(showtime.Id);
        if (bookedSeats > 0)
        {
            return ApiResponse<bool>.FailureResponse(
                "Cannot delete a showtime that already has sold tickets",
                400,
                [new ErrorDetail("SHOWTIME_HAS_BOOKINGS", "This showtime already has sold tickets", "Id")]);
        }

        if (showtime.StartTime < DateTime.UtcNow)
        {
            return ApiResponse<bool>.FailureResponse(
                "Cannot delete past showtimes",
                400,
                new List<ErrorDetail>
                {
                    new ErrorDetail("PAST_SHOWTIME", "This showtime has already started or passed", "Id")
                }
            );
        }

        var deleted = await _showtimeRepository.DeleteAsync(id);
        return ApiResponse<bool>.SuccessResponse(deleted, "Showtime deleted successfully");
    }

    public async Task<ApiResponse<List<ShowtimeDto>>> GetUpcomingShowtimesAsync(int count = 20)
    {
        var showtimes = await _showtimeRepository.GetUpcomingShowtimesAsync(DateTime.UtcNow, count);
        var dtos = new List<ShowtimeDto>();

        foreach (var showtime in showtimes)
        {
            var cinemaHallInfo = await _cinemaApiClient.GetCinemaHallInfoAsync(showtime.CinemaHallId);
            dtos.Add(MapToDto(showtime, cinemaHallInfo));
        }

        return ApiResponse<List<ShowtimeDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<List<ShowtimeLookupItemDto>>> GetLookupByIdsAsync(List<Guid> showtimeIds)
    {
        var normalizedIds = showtimeIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (normalizedIds.Count == 0)
        {
            return ApiResponse<List<ShowtimeLookupItemDto>>.ValidationErrorResponse(
                "Validation failed",
                [new ErrorDetail("SHOWTIME_IDS_REQUIRED", "At least one showtime id is required", "ShowtimeIds")]);
        }

        var showtimes = await _showtimeRepository.GetByIdsAsync(normalizedIds);
        var dtos = showtimes
            .Select(MapToLookupDto)
            .ToList();

        return ApiResponse<List<ShowtimeLookupItemDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<List<ShowtimeLookupItemDto>>> GetByRangeAsync(DateTime from, DateTime to)
    {
        if (from >= to)
        {
            return ApiResponse<List<ShowtimeLookupItemDto>>.ValidationErrorResponse(
                "Validation failed",
                [new ErrorDetail("INVALID_TIME_RANGE", "'from' must be earlier than 'to'.", "from")]);
        }

        var showtimes = await _showtimeRepository.GetByTimeRangeAsync(from, to);
        var dtos = showtimes
            .Select(MapToLookupDto)
            .ToList();

        return ApiResponse<List<ShowtimeLookupItemDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<ShowtimeTimelineDto>> GetTimelineAsync(Guid cinemaId, DateTime date)
    {
        var cinema = await _cinemaApiClient.GetCinemaInfoAsync(cinemaId);
        if (cinema == null)
        {
            return ApiResponse<ShowtimeTimelineDto>.NotFoundResponse("Cinema not found");
        }

        var halls = await _cinemaApiClient.GetCinemaHallsByCinemaIdAsync(cinemaId);
        var timelineStart = date.Date.AddHours(_schedulingOptions.TimelineStartHour);
        var timelineEnd = date.Date.AddDays(1).AddHours(_schedulingOptions.TimelineEndHourNextDay);

        var hallIds = halls.Select(hall => hall.Id).ToList();
        var showtimes = await _showtimeRepository.GetByCinemaHallIdsAndTimeRangeAsync(hallIds, timelineStart, timelineEnd);
        var occupancyMap = await _bookingApiClient.GetShowtimeOccupancyAsync(showtimes.Select(showtime => showtime.Id).ToList());

        var rooms = halls
            .OrderBy(hall => hall.Name)
            .Select(hall => new TimelineRoomDto
            {
                RoomId = hall.Id,
                RoomName = hall.Name,
                TotalSeats = hall.TotalSeats,
                Showtimes = showtimes
                    .Where(showtime => showtime.CinemaHallId == hall.Id)
                    .Select(showtime =>
                    {
                        var bookedSeats = occupancyMap.GetValueOrDefault(showtime.Id);
                        var occupancyRate = hall.TotalSeats <= 0
                            ? 0
                            : Math.Round((decimal)bookedSeats * 100 / hall.TotalSeats, 2);

                        return new TimelineShowtimeDto
                        {
                            Id = showtime.Id,
                            MovieId = showtime.MovieId,
                            MovieTitle = showtime.Movie.Title,
                            Start = showtime.StartTime,
                            End = showtime.EndTime,
                            CleaningEnd = showtime.EndTime.AddMinutes(_schedulingOptions.CleaningBufferMinutes),
                            DurationMinutes = (int)(showtime.EndTime - showtime.StartTime).TotalMinutes,
                            CleaningBufferMinutes = _schedulingOptions.CleaningBufferMinutes,
                            Price = showtime.Price,
                            TotalSeats = hall.TotalSeats,
                            BookedSeats = bookedSeats,
                            OccupancyRate = occupancyRate,
                            HasBookings = bookedSeats > 0,
                            CanReschedule = bookedSeats == 0 && showtime.StartTime > DateTime.UtcNow
                        };
                    })
                    .OrderBy(item => item.Start)
                    .ToList()
            })
            .ToList();

        var response = new ShowtimeTimelineDto
        {
            CinemaId = cinema.Id,
            CinemaName = cinema.Name,
            Date = date.Date,
            TimelineStart = timelineStart,
            TimelineEnd = timelineEnd,
            CleaningBufferMinutes = _schedulingOptions.CleaningBufferMinutes,
            Rooms = rooms
        };

        return ApiResponse<ShowtimeTimelineDto>.SuccessResponse(response);
    }

    public async Task<ApiResponse<ShowtimeConflictValidationResponse>> ValidateSlotAsync(ValidateShowtimeSlotRequest request)
    {
        var validationResult = await ValidateCreateRequestAsync(new CreateShowtimeRequest
        {
            MovieId = request.MovieId,
            CinemaHallId = request.CinemaHallId,
            StartTime = request.StartTime,
            Price = 50000m  // Use realistic default price for validation (50,000 VND)
        });

        if (!validationResult.Success)
        {
            return ApiResponse<ShowtimeConflictValidationResponse>.FailureResponse(
                validationResult.Message,
                validationResult.StatusCode,
                validationResult.Errors);
        }

        var movie = await _movieRepository.GetByIdAsync(request.MovieId);
        if (movie == null)
        {
            return ApiResponse<ShowtimeConflictValidationResponse>.NotFoundResponse("Movie not found");
        }

        var proposedEnd = request.StartTime.AddMinutes(movie.Duration);
        var proposedCleaningEnd = proposedEnd.AddMinutes(_schedulingOptions.CleaningBufferMinutes);

        var conflicts = await _showtimeRepository.GetConflictingShowtimesAsync(
            request.CinemaHallId,
            request.StartTime,
            proposedCleaningEnd,
            _schedulingOptions.CleaningBufferMinutes,
            request.ExcludeShowtimeId);

        var response = new ShowtimeConflictValidationResponse
        {
            IsAvailable = conflicts.Count == 0,
            MovieId = request.MovieId,
            CinemaHallId = request.CinemaHallId,
            ProposedStartTime = request.StartTime,
            ProposedEndTime = proposedEnd,
            ProposedCleaningEnd = proposedCleaningEnd,
            CleaningBufferMinutes = _schedulingOptions.CleaningBufferMinutes,
            Conflicts = conflicts.Select(conflict => new ShowtimeConflictItemDto
            {
                ShowtimeId = conflict.Id,
                MovieId = conflict.MovieId,
                MovieTitle = conflict.Movie.Title,
                StartTime = conflict.StartTime,
                EndTime = conflict.EndTime,
                CleaningEndTime = conflict.EndTime.AddMinutes(_schedulingOptions.CleaningBufferMinutes)
            }).ToList()
        };

        return ApiResponse<ShowtimeConflictValidationResponse>.SuccessResponse(response);
    }

    private async Task<ApiResponse<bool>> ValidateCreateRequestAsync(CreateShowtimeRequest request)
    {
        var errors = new List<ErrorDetail>();

        if (request.StartTime < DateTime.UtcNow.AddHours(1))
        {
            errors.Add(new ErrorDetail(
                "INVALID_START_TIME",
                "Showtime must be scheduled at least 1 hour in advance",
                "StartTime"
            ));
        }

        var cinemaHallExists = await _cinemaApiClient.ValidateCinemaHallExistsAsync(request.CinemaHallId);
        if (!cinemaHallExists)
        {
            errors.Add(new ErrorDetail(
                "INVALID_CINEMA_HALL",
                "Cinema hall does not exist",
                "CinemaHallId"
            ));
        }

        if (errors.Any())
        {
            return ApiResponse<bool>.ValidationErrorResponse("Validation failed", errors);
        }

        return ApiResponse<bool>.SuccessResponse(true);
    }

    private async Task<int> GetBookedSeatsAsync(Guid showtimeId)
    {
        var occupancyMap = await _bookingApiClient.GetShowtimeOccupancyAsync([showtimeId]);
        return occupancyMap.GetValueOrDefault(showtimeId);
    }

    private ShowtimeDto MapToDto(Showtime showtime, CinemaHallInfo? cinemaHallInfo)
    {
        return new ShowtimeDto
        {
            Id = showtime.Id,
            MovieId = showtime.MovieId,
            MovieTitle = showtime.Movie.Title,
            CinemaHallId = showtime.CinemaHallId,
            CinemaHallName = cinemaHallInfo?.Name,
            CinemaName = cinemaHallInfo?.CinemaName,
            StartTime = showtime.StartTime,
            EndTime = showtime.EndTime,
            Price = showtime.Price,
            DurationMinutes = (int)(showtime.EndTime - showtime.StartTime).TotalMinutes,
            CreatedAt = showtime.CreatedAt
        };
    }

    private async Task<ShowtimeDetailDto> MapToDetailDtoAsync(Showtime showtime, CinemaHallInfo? cinemaHallInfo)
    {
        var movieDto = new MovieDto
        {
            Id = showtime.Movie.Id,
            Title = showtime.Movie.Title,
            Description = showtime.Movie.Description,
            Duration = showtime.Movie.Duration,
            Language = showtime.Movie.Language,
            ReleaseDate = showtime.Movie.ReleaseDate,
            PosterUrl = showtime.Movie.PosterUrl,
            CreatedAt = showtime.Movie.CreatedAt,
            Genres = showtime.Movie.MovieGenres.Select(mg => new GenreDto
            {
                Id = mg.Genre.Id,
                Name = mg.Genre.Name
            }).ToList()
        };

        return new ShowtimeDetailDto
        {
            Id = showtime.Id,
            MovieId = showtime.MovieId,
            MovieTitle = showtime.Movie.Title,
            CinemaHallId = showtime.CinemaHallId,
            CinemaHallName = cinemaHallInfo?.Name,
            CinemaName = cinemaHallInfo?.CinemaName,
            StartTime = showtime.StartTime,
            EndTime = showtime.EndTime,
            Price = showtime.Price,
            DurationMinutes = (int)(showtime.EndTime - showtime.StartTime).TotalMinutes,
            CreatedAt = showtime.CreatedAt,
            Movie = movieDto,
            TotalSeats = cinemaHallInfo?.TotalSeats ?? 0,
            AvailableSeats = cinemaHallInfo?.TotalSeats ?? 0
        };
    }

    private static ShowtimeLookupItemDto MapToLookupDto(Showtime showtime)
    {
        return new ShowtimeLookupItemDto
        {
            ShowtimeId = showtime.Id,
            MovieId = showtime.MovieId,
            MovieTitle = showtime.Movie.Title,
            PosterUrl = showtime.Movie.PosterUrl,
            CinemaHallId = showtime.CinemaHallId,
            StartTime = showtime.StartTime,
            EndTime = showtime.EndTime,
            Price = showtime.Price
        };
    }
}


