using System.Collections.Concurrent;
using Booking.API.Client;
using Booking.API.Entities;
using Booking.API.Repositories;
using Booking.API.Services;
using ICinemaApiClient = Cinema.API.Client.Client.ICinemaApiClient;
using IMovieApiClient = Movie.API.Client.Client.IMovieApiClient;
using DomainBookingStatus = Booking.API.Entities.BookingStatus;

namespace Booking.API.Infrastructure.Caching.Services;

/// <summary>
/// Temporary in-process replacement while Redis is disabled for this refactor.
/// </summary>
public class InMemorySeatStatusService : ISeatStatusService
{
    private const int DefaultLockMinutes = 10;
    private static readonly ConcurrentDictionary<(Guid ShowtimeId, Guid SeatId), SeatState> SeatStates = new();

    private readonly IMovieApiClient _movieApiClient;
    private readonly ICinemaApiClient _cinemaApiClient;
    private readonly IBookingRepository _bookingRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<InMemorySeatStatusService> _logger;

    public InMemorySeatStatusService(
        IMovieApiClient movieApiClient,
        ICinemaApiClient cinemaApiClient,
        IBookingRepository bookingRepository,
        IConfiguration configuration,
        ILogger<InMemorySeatStatusService> logger)
    {
        _movieApiClient = movieApiClient ?? throw new ArgumentNullException(nameof(movieApiClient));
        _cinemaApiClient = cinemaApiClient ?? throw new ArgumentNullException(nameof(cinemaApiClient));
        _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SeatAvailabilityResponse> GetSeatAvailabilityAsync(Guid showtimeId)
    {
        var showtimeResponse = await _movieApiClient.GetShowtimeByIdAsync(showtimeId);
        var showtime = showtimeResponse.Success && showtimeResponse.Data is not null
            ? ExternalClientDtoMapper.ToBookingShowtime(showtimeResponse.Data)
            : throw new InvalidOperationException($"Showtime {showtimeId} not found");

        var hallResponse = await _cinemaApiClient.GetHallByIdAsync(showtime.CinemaHallId);
        var hall = hallResponse.Success && hallResponse.Data is not null
            ? ExternalClientDtoMapper.ToBookingCinemaHall(hallResponse.Data)
            : throw new InvalidOperationException($"Cinema hall {showtime.CinemaHallId} not found");

        var seatResponse = await _cinemaApiClient.GetHallSeatsAsync(showtime.CinemaHallId);
        var seats = seatResponse.Success && seatResponse.Data is not null
            ? seatResponse.Data.Select(ExternalClientDtoMapper.ToBookingSeat).ToList()
            : [];
        var bookedSeatIds = await GetBookedSeatIdsAsync(showtimeId);
        CleanupExpiredLocks(showtimeId);

        var seatStatuses = seats.Select(seat =>
        {
            var state = GetState(showtimeId, seat.Id, bookedSeatIds);

            return new SeatStatusDto
            {
                SeatId = seat.Id,
                Row = seat.Row,
                Number = seat.Number,
                Price = showtime.Price,
                Status = state.Status,
                LockedBy = state.UserId,
                LockedUntil = state.LockedUntil
            };
        }).ToList();

        return new SeatAvailabilityResponse
        {
            ShowtimeId = showtimeId,
            CinemaHallId = hall.Id,
            CinemaHallName = hall.Name,
            Seats = seatStatuses,
            Summary = new SeatAvailabilitySummary
            {
                TotalSeats = seatStatuses.Count,
                AvailableSeats = seatStatuses.Count(seat => seat.Status == SeatStatus.Available),
                LockedSeats = seatStatuses.Count(seat => seat.Status == SeatStatus.Locked),
                BookedSeats = seatStatuses.Count(seat => seat.Status == SeatStatus.Booked)
            }
        };
    }

    public Task InitializeSeatMapAsync(Guid showtimeId, Guid cinemaHallId)
    {
        return Task.CompletedTask;
    }

    public async Task<SeatLockResult> LockSeatsAsync(Guid showtimeId, List<Guid> seatIds, string userId)
    {
        CleanupExpiredLocks(showtimeId);
        var bookedSeatIds = await GetBookedSeatIdsAsync(showtimeId);
        var result = new SeatLockResult { Success = true };
        var lockUntil = DateTime.UtcNow.AddMinutes(GetLockMinutes());

        foreach (var seatId in seatIds.Distinct())
        {
            var key = (showtimeId, seatId);
            var current = GetState(showtimeId, seatId, bookedSeatIds);

            if (current.Status == SeatStatus.Booked ||
                current.Status == SeatStatus.Locked && current.UserId != userId)
            {
                result.Success = false;
                result.AlreadyLockedSeats.Add(seatId);
                continue;
            }

            SeatStates[key] = new SeatState(SeatStatus.Locked, userId, null, lockUntil);
            result.LockedSeats.Add(seatId);
        }

        result.Message = result.Success ? "Seats locked successfully" : "Some seats are not available";
        return result;
    }

    public Task<bool> UnlockSeatsAsync(Guid showtimeId, List<Guid> seatIds, string userId)
    {
        foreach (var seatId in seatIds.Distinct())
        {
            var key = (showtimeId, seatId);
            if (SeatStates.TryGetValue(key, out var state) &&
                state.Status == SeatStatus.Locked &&
                state.UserId == userId)
            {
                SeatStates.TryRemove(key, out _);
            }
        }

        return Task.FromResult(true);
    }

    public Task<bool> MarkSeatsAsBookedAsync(Guid showtimeId, List<Guid> seatIds, Guid bookingId)
    {
        foreach (var seatId in seatIds.Distinct())
        {
            SeatStates[(showtimeId, seatId)] = new SeatState(SeatStatus.Booked, null, bookingId, null);
        }

        return Task.FromResult(true);
    }

    public Task<bool> ReleaseBookedSeatsAsync(Guid showtimeId, List<Guid> seatIds)
    {
        foreach (var seatId in seatIds.Distinct())
        {
            SeatStates.TryRemove((showtimeId, seatId), out _);
        }

        return Task.FromResult(true);
    }

    public async Task<bool> AreSeatsAvailableAsync(Guid showtimeId, List<Guid> seatIds)
    {
        CleanupExpiredLocks(showtimeId);
        var bookedSeatIds = await GetBookedSeatIdsAsync(showtimeId);

        return seatIds.Distinct().All(seatId =>
            GetState(showtimeId, seatId, bookedSeatIds).Status == SeatStatus.Available);
    }

    public async Task<SeatStatusInfo> GetSeatStatusAsync(Guid showtimeId, Guid seatId)
    {
        CleanupExpiredLocks(showtimeId);
        var bookedSeatIds = await GetBookedSeatIdsAsync(showtimeId);
        var state = GetState(showtimeId, seatId, bookedSeatIds);

        return new SeatStatusInfo
        {
            SeatId = seatId,
            Status = state.Status,
            UserId = state.UserId,
            BookingId = state.BookingId,
            LockedUntil = state.LockedUntil
        };
    }

    public Task<bool> ExtendSeatLocksAsync(Guid showtimeId, List<Guid> seatIds, string userId)
    {
        var lockUntil = DateTime.UtcNow.AddMinutes(GetLockMinutes());
        var success = true;

        foreach (var seatId in seatIds.Distinct())
        {
            var key = (showtimeId, seatId);
            if (!SeatStates.TryGetValue(key, out var state) ||
                state.Status != SeatStatus.Locked ||
                state.UserId != userId)
            {
                success = false;
                continue;
            }

            SeatStates[key] = state with { LockedUntil = lockUntil };
        }

        return Task.FromResult(success);
    }

    public async Task<SeatBookingResult> VerifyAndMarkAsBookedAsync(
        Guid showtimeId,
        List<Guid> seatIds,
        string userId,
        Guid bookingId)
    {
        CleanupExpiredLocks(showtimeId);
        var bookedSeatIds = await GetBookedSeatIdsAsync(showtimeId);
        var result = new SeatBookingResult { Success = true };

        foreach (var seatId in seatIds.Distinct())
        {
            var state = GetState(showtimeId, seatId, bookedSeatIds);
            if (state.Status == SeatStatus.Booked)
            {
                result.Success = false;
                result.FailedSeats.Add(seatId);
                result.FailureReason = SeatBookingFailureReason.AlreadyBooked;
                continue;
            }

            if (state.Status == SeatStatus.Locked && state.UserId != userId)
            {
                result.Success = false;
                result.FailedSeats.Add(seatId);
                result.FailureReason = SeatBookingFailureReason.WrongUser;
                continue;
            }

            SeatStates[(showtimeId, seatId)] = new SeatState(SeatStatus.Booked, null, bookingId, null);
            result.BookedSeats.Add(seatId);
        }

        result.Message = result.Success ? "Seats booked successfully" : "Some seats are not available";
        return result;
    }

    public Task CleanupExpiredLocksAsync(Guid showtimeId)
    {
        CleanupExpiredLocks(showtimeId);
        return Task.CompletedTask;
    }

    private async Task<HashSet<Guid>> GetBookedSeatIdsAsync(Guid showtimeId)
    {
        var bookings = await _bookingRepository.GetByShowtimeIdAsync(showtimeId);

        return bookings
            .Where(booking => booking.Status is DomainBookingStatus.Pending or DomainBookingStatus.Confirmed or DomainBookingStatus.CheckedIn)
            .SelectMany(booking => booking.BookingSeats.Select(seat => seat.SeatId))
            .ToHashSet();
    }

    private static SeatState GetState(Guid showtimeId, Guid seatId, HashSet<Guid> bookedSeatIds)
    {
        if (bookedSeatIds.Contains(seatId))
        {
            return new SeatState(SeatStatus.Booked, null, null, null);
        }

        if (!SeatStates.TryGetValue((showtimeId, seatId), out var state))
        {
            return SeatState.Available;
        }

        if (state.Status == SeatStatus.Locked && state.LockedUntil <= DateTime.UtcNow)
        {
            SeatStates.TryRemove((showtimeId, seatId), out _);
            return SeatState.Available;
        }

        return state;
    }

    private static void CleanupExpiredLocks(Guid showtimeId)
    {
        var now = DateTime.UtcNow;

        foreach (var item in SeatStates)
        {
            if (item.Key.ShowtimeId == showtimeId &&
                item.Value.Status == SeatStatus.Locked &&
                item.Value.LockedUntil <= now)
            {
                SeatStates.TryRemove(item.Key, out _);
            }
        }
    }

    private int GetLockMinutes()
    {
        var minutes = _configuration.GetValue<int>("SeatLock:LockDurationMinutes", DefaultLockMinutes);
        if (minutes <= 0)
        {
            _logger.LogWarning("Seat lock duration must be positive. Falling back to {Minutes} minutes.", DefaultLockMinutes);
            return DefaultLockMinutes;
        }

        return minutes;
    }

    private sealed record SeatState(
        SeatStatus Status,
        string? UserId,
        Guid? BookingId,
        DateTime? LockedUntil)
    {
        public static readonly SeatState Available = new(SeatStatus.Available, null, null, null);
    }
}
