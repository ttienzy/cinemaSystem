using Cinema.Shared.Models;
using Microsoft.Extensions.Options;
using Movie.API.Application.DTOs;
using Movie.API.Application.Mappers;
using Movie.API.Domain.Entities;
using Movie.API.Domain.Exceptions;
using Movie.API.Infrastructure.Persistence.Repositories;

namespace Movie.API.Application.Services;

public class ShowtimeService : IShowtimeService
{
    private const decimal ValidationDefaultPrice = 50000m;
    private static readonly TimeZoneInfo VietnamTimeZone = ResolveVietnamTimeZone();

    private readonly IShowtimeRepository _showtimeRepository;
    private readonly IMovieRepository _movieRepository;
    private readonly ICinemaApiClient _cinemaApiClient;
    private readonly IBookingApiClient _bookingApiClient;
    private readonly SchedulingOptions _schedulingOptions;
    private readonly ILogger<ShowtimeService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public ShowtimeService(
        IShowtimeRepository showtimeRepository,
        IMovieRepository movieRepository,
        ICinemaApiClient cinemaApiClient,
        IBookingApiClient bookingApiClient,
        IOptions<SchedulingOptions> schedulingOptions,
        ILogger<ShowtimeService> logger,
        IUnitOfWork unitOfWork)
    {
        _showtimeRepository = showtimeRepository;
        _movieRepository = movieRepository;
        _cinemaApiClient = cinemaApiClient;
        _bookingApiClient = bookingApiClient;
        _schedulingOptions = schedulingOptions.Value;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<ShowtimeDetailDto>> GetByIdAsync(Guid id)
    {
        return await ExecuteSafelyAsync(nameof(GetByIdAsync), async () =>
        {
            var showtime = await _showtimeRepository.GetByIdAsync(id);
            if (showtime == null)
            {
                return ApiResponse<ShowtimeDetailDto>.NotFoundResponse(ShowtimeException.SHOWTIME_NOT_FOUND);
            }

            var cinemaHallInfo = await _cinemaApiClient.GetCinemaHallInfoAsync(showtime.CinemaHallId);
            var dto = showtime.ShowtimeDetailMapToDto(cinemaHallInfo, DateTime.UtcNow);

            return ApiResponse<ShowtimeDetailDto>.SuccessResponse(dto);
        });
    }

    public async Task<ApiResponse<List<ShowtimeDto>>> GetByMovieIdAsync(Guid movieId)
    {
        return await ExecuteSafelyAsync(nameof(GetByMovieIdAsync), async () =>
        {
            var movie = await _movieRepository.GetByIdAsync(movieId);
            if (movie == null)
            {
                return ApiResponse<List<ShowtimeDto>>.NotFoundResponse(MovieException.MOVIE_NOT_FOUND);
            }

            var showtimes = await _showtimeRepository.GetByMovieIdAsync(movieId);
            var dtos = await MapShowtimeDtosAsync(showtimes);

            return ApiResponse<List<ShowtimeDto>>.SuccessResponse(dtos);
        });
    }

    public async Task<ApiResponse<List<ShowtimeDto>>> GetByCinemaHallIdAsync(Guid cinemaHallId)
    {
        return await ExecuteSafelyAsync(nameof(GetByCinemaHallIdAsync), async () =>
        {
            var cinemaHallExists = await _cinemaApiClient.ValidateCinemaHallExistsAsync(cinemaHallId);
            if (!cinemaHallExists)
            {
                return ApiResponse<List<ShowtimeDto>>.NotFoundResponse(ShowtimeException.CINEMA_HALL_NOT_FOUND);
            }

            var showtimes = await _showtimeRepository.GetByCinemaHallIdAsync(cinemaHallId);
            var cinemaHallInfo = await _cinemaApiClient.GetCinemaHallInfoAsync(cinemaHallId);
            var dtos = showtimes.Select(showtime => showtime.ShowtimeMapToDto(cinemaHallInfo)).ToList();

            return ApiResponse<List<ShowtimeDto>>.SuccessResponse(dtos);
        });
    }

    public async Task<ApiResponse<List<ShowtimeLookupItemDto>>> GetLookupByIdsAsync(List<Guid> showtimeIds)
    {
        return await ExecuteSafelyAsync(nameof(GetLookupByIdsAsync), async () =>
        {
            var normalizedIds = showtimeIds
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            if (normalizedIds.Count == 0)
            {
                var value = ShowtimeException.SHOWTIME_IDS_REQUIRED;
                return ApiResponse<List<ShowtimeLookupItemDto>>.ValidationErrorResponse(
                    ShowtimeException.VALIDATION_FAILED,
                    [new ErrorDetail(value.Item1, value.Item2, value.Item3)]);
            }

            var showtimes = await _showtimeRepository.GetByIdsAsync(normalizedIds);
            var dtos = showtimes
                .Select(showtime => showtime.ShowtimeMapToLookupDto())
                .ToList();

            return ApiResponse<List<ShowtimeLookupItemDto>>.SuccessResponse(dtos);
        });
    }

    public async Task<ApiResponse<List<ShowtimeLookupItemDto>>> GetByRangeAsync(DateTime from, DateTime to)
    {
        return await ExecuteSafelyAsync(nameof(GetByRangeAsync), async () =>
        {
            if (from >= to)
            {
                var value = ShowtimeException.INVALID_TIME_RANGE;
                return ApiResponse<List<ShowtimeLookupItemDto>>.ValidationErrorResponse(
                    ShowtimeException.VALIDATION_FAILED,
                    [new ErrorDetail(value.Item1, value.Item2, value.Item3)]);
            }

            var showtimes = await _showtimeRepository.GetByTimeRangeAsync(from, to);
            var dtos = showtimes
                .Select(showtime => showtime.ShowtimeMapToLookupDto())
                .ToList();

            return ApiResponse<List<ShowtimeLookupItemDto>>.SuccessResponse(dtos);
        });
    }

    public async Task<ApiResponse<List<ShowtimeDto>>> GetUpcomingShowtimesAsync(int count = 20)
    {
        return await ExecuteSafelyAsync(nameof(GetUpcomingShowtimesAsync), async () =>
        {
            var showtimes = await _showtimeRepository.GetUpcomingShowtimesAsync(DateTime.UtcNow, count);
            var dtos = await MapShowtimeDtosAsync(showtimes);

            return ApiResponse<List<ShowtimeDto>>.SuccessResponse(dtos);
        });
    }

    public async Task<ApiResponse<ShowtimeTimelineDto>> GetTimelineAsync(Guid cinemaId, DateTime date)
    {
        return await ExecuteSafelyAsync(nameof(GetTimelineAsync), async () =>
        {
            var cinema = await _cinemaApiClient.GetCinemaInfoAsync(cinemaId);
            if (cinema == null)
            {
                return ApiResponse<ShowtimeTimelineDto>.NotFoundResponse(ShowtimeException.CINEMA_NOT_FOUND);
            }

            var halls = await _cinemaApiClient.GetCinemaHallsByCinemaIdAsync(cinemaId);

            // Ensure date is treated as local date, then convert to UTC for database query
            var localDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Local);
            var timelineStartLocal = localDate.AddHours(_schedulingOptions.TimelineStartHour);
            var timelineEndLocal = localDate.AddDays(1).AddHours(_schedulingOptions.TimelineEndHourNextDay);

            // Convert to UTC for database query
            var timelineStart = timelineStartLocal.ToUniversalTime();
            var timelineEnd = timelineEndLocal.ToUniversalTime();

            var hallIds = halls.Select(hall => hall.Id).ToList();
            var showtimes = await _showtimeRepository.GetByCinemaHallIdsAndTimeRangeAsync(hallIds, timelineStart, timelineEnd);
            var occupancyMap = await _bookingApiClient.GetShowtimeOccupancyAsync(showtimes.Select(showtime => showtime.Id).ToList());
            var now = DateTime.UtcNow;

            var rooms = halls
                .OrderBy(hall => hall.Name)
                .Select(hall => new TimelineRoomDto
                {
                    RoomId = hall.Id,
                    RoomName = hall.Name,
                    TotalSeats = hall.TotalSeats,
                    Showtimes = showtimes
                        .Where(showtime => showtime.CinemaHallId == hall.Id)
                        .Select(showtime => showtime.ShowtimeMapToTimelineItemDto(
                            _schedulingOptions.CleaningBufferMinutes,
                            hall.TotalSeats,
                            occupancyMap.GetValueOrDefault(showtime.Id),
                            now))
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
        });
    }

    public async Task<ApiResponse<ShowtimeConflictValidationResponse>> ValidateSlotAsync(ValidateShowtimeSlotRequest request)
    {
        return await ExecuteSafelyAsync(nameof(ValidateSlotAsync), async () =>
        {
            var validationResult = await ValidateCreateRequestAsync(new CreateShowtimeRequest
            {
                MovieId = request.MovieId,
                CinemaHallId = request.CinemaHallId,
                StartTime = request.StartTime,
                Price = ValidationDefaultPrice
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
                return ApiResponse<ShowtimeConflictValidationResponse>.NotFoundResponse(MovieException.MOVIE_NOT_FOUND);
            }

            var proposedShowtime = Showtime.Create(
                request.MovieId,
                request.CinemaHallId,
                request.StartTime,
                movie.Duration,
                ValidationDefaultPrice);

            var conflicts = await _showtimeRepository.GetConflictingShowtimesAsync(
                request.CinemaHallId,
                proposedShowtime.StartTime,
                proposedShowtime.GetCleaningEndTime(_schedulingOptions.CleaningBufferMinutes),
                _schedulingOptions.CleaningBufferMinutes,
                request.ExcludeShowtimeId);

            var response = new ShowtimeConflictValidationResponse
            {
                IsAvailable = conflicts.Count == 0,
                MovieId = request.MovieId,
                CinemaHallId = request.CinemaHallId,
                ProposedStartTime = proposedShowtime.StartTime,
                ProposedEndTime = proposedShowtime.EndTime,
                ProposedCleaningEnd = proposedShowtime.GetCleaningEndTime(_schedulingOptions.CleaningBufferMinutes),
                CleaningBufferMinutes = _schedulingOptions.CleaningBufferMinutes,
                Conflicts = conflicts
                    .Select(conflict => conflict.ShowtimeMapToConflictItemDto(_schedulingOptions.CleaningBufferMinutes))
                    .ToList()
            };

            return ApiResponse<ShowtimeConflictValidationResponse>.SuccessResponse(response);
        });
    }

    public async Task<ApiResponse<ShowtimeDto>> CreateAsync(CreateShowtimeRequest request)
    {
        return await ExecuteSafelyAsync(nameof(CreateAsync), async () =>
        {
            var validationResult = await ValidateCreateRequestAsync(request);
            if (!validationResult.Success)
            {
                return ApiResponse<ShowtimeDto>.FailureResponse(
                    validationResult.Message,
                    validationResult.StatusCode,
                    validationResult.Errors);
            }

            var movie = await _movieRepository.GetByIdAsync(request.MovieId);
            if (movie == null)
            {
                return ApiResponse<ShowtimeDto>.NotFoundResponse(MovieException.MOVIE_NOT_FOUND);
            }

            var showtime = Showtime.Create(
                request.MovieId,
                request.CinemaHallId,
                request.StartTime,
                movie.Duration,
                request.Price);

            var hasOverlap = await HasOverlapAsync(showtime);
            if (hasOverlap)
            {
                var value = ShowtimeException.SHOWTIME_OVERLAP;
                return ApiResponse<ShowtimeDto>.FailureResponse(
                    ShowtimeException.SHOWTIME_OVERLAP_MESSAGE,
                    400,
                    [new ErrorDetail(value.Item1, value.Item2, value.Item3)]);
            }

            await ExecuteInTransactionAsync(nameof(CreateAsync), async () =>
            {
                await _showtimeRepository.CreateAsync(showtime);
            });

            var created = await GetRequiredShowtimeAsync(showtime.Id);
            var cinemaHallInfo = await _cinemaApiClient.GetCinemaHallInfoAsync(created.CinemaHallId);
            var dto = created.ShowtimeMapToDto(cinemaHallInfo);

            _logger.LogInformation(
                "Showtime created: {ShowtimeId} for Movie {MovieId} at Hall {CinemaHallId}",
                created.Id, created.MovieId, created.CinemaHallId);

            return ApiResponse<ShowtimeDto>.SuccessResponse(dto, ShowtimeException.SHOWTIME_CREATED_SUCCESSFULLY, 201);
        });
    }

    public async Task<ApiResponse<ShowtimeDto>> UpdateAsync(Guid id, UpdateShowtimeRequest request)
    {
        return await ExecuteSafelyAsync(nameof(UpdateAsync), async () =>
        {
            var existing = await _showtimeRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return ApiResponse<ShowtimeDto>.NotFoundResponse(ShowtimeException.SHOWTIME_NOT_FOUND);
            }

            var bookedSeats = await GetBookedSeatsAsync(existing.Id);
            if (existing.HasBookings(bookedSeats))
            {
                var value = ShowtimeException.SHOWTIME_HAS_BOOKINGS;
                return ApiResponse<ShowtimeDto>.FailureResponse(
                    ShowtimeException.CANNOT_UPDATE_HAS_TICKETS,
                    400,
                    [new ErrorDetail(value.Item1, value.Item2, value.Item3)]);
            }

            if (existing.HasStarted(DateTime.UtcNow))
            {
                var value = ShowtimeException.PAST_SHOWTIME;
                return ApiResponse<ShowtimeDto>.FailureResponse(
                    ShowtimeException.CANNOT_UPDATE_PAST_SHOWTIME,
                    400,
                    [new ErrorDetail(value.Item1, value.Item2, value.Item3)]);
            }

            var movie = await _movieRepository.GetByIdAsync(existing.MovieId);
            if (movie == null)
            {
                return ApiResponse<ShowtimeDto>.NotFoundResponse(MovieException.MOVIE_NOT_FOUND);
            }

            var updatedShowtime = Showtime.Create(
                existing.MovieId,
                existing.CinemaHallId,
                request.StartTime,
                movie.Duration,
                request.Price);

            var hasOverlap = await HasOverlapAsync(updatedShowtime, id);
            if (hasOverlap)
            {
                var value = ShowtimeException.SHOWTIME_OVERLAP;
                return ApiResponse<ShowtimeDto>.FailureResponse(
                    ShowtimeException.SHOWTIME_OVERLAP_MESSAGE,
                    400,
                    [new ErrorDetail(value.Item1, value.Item2, value.Item3)]);
            }

            await ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
            {
                var updated = await _showtimeRepository.UpdateAsync(id, updatedShowtime);
                if (updated == null)
                {
                    throw new InvalidOperationException($"Showtime {id} could not be updated.");
                }
            });

            var persistedShowtime = await GetRequiredShowtimeAsync(id);
            var cinemaHallInfo = await _cinemaApiClient.GetCinemaHallInfoAsync(persistedShowtime.CinemaHallId);
            var dto = persistedShowtime.ShowtimeMapToDto(cinemaHallInfo);

            return ApiResponse<ShowtimeDto>.SuccessResponse(dto, ShowtimeException.SHOWTIME_UPDATED_SUCCESSFULLY);
        });
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        return await ExecuteSafelyAsync(nameof(DeleteAsync), async () =>
        {
            var showtime = await _showtimeRepository.GetByIdAsync(id);
            if (showtime == null)
            {
                return ApiResponse<bool>.NotFoundResponse(ShowtimeException.SHOWTIME_NOT_FOUND);
            }

            var bookedSeats = await GetBookedSeatsAsync(showtime.Id);
            if (showtime.HasBookings(bookedSeats))
            {
                var value = ShowtimeException.SHOWTIME_HAS_BOOKINGS;
                return ApiResponse<bool>.FailureResponse(
                    ShowtimeException.CANNOT_DELETE_HAS_TICKETS,
                    400,
                    [new ErrorDetail(value.Item1, value.Item2, value.Item3)]);
            }

            if (showtime.HasStarted(DateTime.UtcNow))
            {
                var value = ShowtimeException.PAST_SHOWTIME;
                return ApiResponse<bool>.FailureResponse(
                    ShowtimeException.CANNOT_DELETE_PAST_SHOWTIME,
                    400,
                    [new ErrorDetail(value.Item1, value.Item2, value.Item3)]);
            }

            await ExecuteInTransactionAsync(nameof(DeleteAsync), async () =>
            {
                var deleted = await _showtimeRepository.DeleteAsync(id);
                if (!deleted)
                {
                    throw new InvalidOperationException($"Showtime {id} could not be deleted.");
                }
            });

            return ApiResponse<bool>.SuccessResponse(true, ShowtimeException.SHOWTIME_DELETED_SUCCESSFULLY);
        });
    }

    private async Task<ApiResponse<bool>> ValidateCreateRequestAsync(CreateShowtimeRequest request)
    {
        var errors = new List<ErrorDetail>();
        var localStartTime = ConvertToVietnamLocalTime(request.StartTime);
        var localNow = ConvertToVietnamLocalTime(DateTime.UtcNow);
        var openingTime = localStartTime.Date.AddHours(_schedulingOptions.TimelineStartHour);

        if (request.StartTime < DateTime.UtcNow.AddHours(1))
        {
            var value = ShowtimeException.INVALID_START_TIME;
            errors.Add(new ErrorDetail(value.Item1, value.Item2, value.Item3));
        }

        if (localStartTime.Date <= localNow.Date)
        {
            var value = ShowtimeException.START_TIME_MUST_BE_AFTER_TODAY;
            errors.Add(new ErrorDetail(value.Item1, value.Item2, value.Item3));
        }

        if (localStartTime <= openingTime)
        {
            var value = ShowtimeException.START_TIME_BEFORE_OPENING_HOUR;
            errors.Add(new ErrorDetail(value.Item1, value.Item2, value.Item3));
        }

        var cinemaHallExists = await _cinemaApiClient.ValidateCinemaHallExistsAsync(request.CinemaHallId);
        if (!cinemaHallExists)
        {
            var value = ShowtimeException.INVALID_CINEMA_HALL;
            errors.Add(new ErrorDetail(value.Item1, value.Item2, value.Item3));
        }

        if (errors.Any())
        {
            return ApiResponse<bool>.ValidationErrorResponse(ShowtimeException.VALIDATION_FAILED, errors);
        }

        return ApiResponse<bool>.SuccessResponse(true);
    }

    private static DateTime ConvertToVietnamLocalTime(DateTime value)
    {
        var utcValue = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        return TimeZoneInfo.ConvertTimeFromUtc(utcValue, VietnamTimeZone);
    }

    private static TimeZoneInfo ResolveVietnamTimeZone()
    {
        var candidateIds = new[] { "SE Asia Standard Time", "Asia/Ho_Chi_Minh" };

        foreach (var timeZoneId in candidateIds)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        return TimeZoneInfo.CreateCustomTimeZone(
            "Vietnam Standard Time",
            TimeSpan.FromHours(7),
            "Vietnam Standard Time",
            "Vietnam Standard Time");
    }

    private async Task<bool> HasOverlapAsync(Showtime showtime, Guid? excludeShowtimeId = null)
    {
        return await _showtimeRepository.HasOverlappingShowtimeAsync(
            showtime.CinemaHallId,
            showtime.StartTime,
            showtime.GetCleaningEndTime(_schedulingOptions.CleaningBufferMinutes),
            _schedulingOptions.CleaningBufferMinutes,
            excludeShowtimeId);
    }

    private async Task<int> GetBookedSeatsAsync(Guid showtimeId)
    {
        var occupancyMap = await _bookingApiClient.GetShowtimeOccupancyAsync([showtimeId]);
        return occupancyMap.GetValueOrDefault(showtimeId);
    }

    private async Task<List<ShowtimeDto>> MapShowtimeDtosAsync(IEnumerable<Showtime> showtimes)
    {
        var showtimeList = showtimes.ToList();
        var cinemaHallInfos = await GetCinemaHallInfoMapAsync(showtimeList.Select(showtime => showtime.CinemaHallId));

        return showtimeList
            .Select(showtime => showtime.ShowtimeMapToDto(cinemaHallInfos.GetValueOrDefault(showtime.CinemaHallId)))
            .ToList();
    }

    private async Task<Dictionary<Guid, CinemaHallInfo?>> GetCinemaHallInfoMapAsync(IEnumerable<Guid> cinemaHallIds)
    {
        var hallIds = cinemaHallIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        var hallInfoTasks = hallIds.Select(async hallId => new
        {
            HallId = hallId,
            Info = await _cinemaApiClient.GetCinemaHallInfoAsync(hallId)
        });

        var hallInfos = await Task.WhenAll(hallInfoTasks);
        return hallInfos.ToDictionary(item => item.HallId, item => item.Info);
    }

    private async Task<Showtime> GetRequiredShowtimeAsync(Guid id)
    {
        return await _showtimeRepository.GetByIdAsync(id)
            ?? throw new InvalidOperationException($"Showtime {id} could not be reloaded.");
    }

    private async Task ExecuteInTransactionAsync(string operationName, Func<Task> action)
    {
        var transactionStarted = false;

        try
        {
            await _unitOfWork.BeginTransactionAsync();
            transactionStarted = true;

            await action();
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            if (transactionStarted)
            {
                await TryRollbackAsync(operationName);
            }

            throw;
        }
    }

    private async Task<ApiResponse<T>> ExecuteSafelyAsync<T>(string operationName, Func<Task<ApiResponse<T>>> action)
    {
        try
        {
            return await action();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error while executing showtime operation {OperationName}", operationName);
            return ApiResponse<T>.InternalServerErrorResponse(ShowtimeException.INTERNAL_SERVER_ERROR);
        }
    }

    private async Task TryRollbackAsync(string operationName)
    {
        try
        {
            await _unitOfWork.RollbackAsync();
        }
        catch (Exception rollbackException)
        {
            _logger.LogError(
                rollbackException,
                "Rollback failed for showtime operation {OperationName}",
                operationName);
        }
    }
}
