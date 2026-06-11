using Booking.API.Entities;
using Booking.API.Infrastructure.Caching.Models;
using Booking.API.Repositories;
using Cinema.API.Client.Client;
using Movie.API.Client.Client;
using BookingEntity = Booking.API.Entities.Booking;
using CinemaSeatDto = Cinema.API.Client.SeatDto;

namespace Booking.API.Services;

public class SeatAvailabilityService : ISeatAvailabilityService
{
    private readonly IMovieApiClient _movieApiClient;
    private readonly ICinemaApiClient _cinemaApiClient;
    private readonly IBookingRepository _bookingRepository;
    private readonly ISeatStatusService _seatStatusService;
    private readonly ILogger<SeatAvailabilityService> _logger;

    public SeatAvailabilityService(
        IMovieApiClient movieApiClient,
        ICinemaApiClient cinemaApiClient,
        IBookingRepository bookingRepository,
        ISeatStatusService seatStatusService,
        ILogger<SeatAvailabilityService> logger)
    {
        _movieApiClient = movieApiClient ?? throw new ArgumentNullException(nameof(movieApiClient));
        _cinemaApiClient = cinemaApiClient ?? throw new ArgumentNullException(nameof(cinemaApiClient));
        _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
        _seatStatusService = seatStatusService ?? throw new ArgumentNullException(nameof(seatStatusService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SeatAvailabilityResponse> GetSeatAvailabilityAsync(
        Guid showtimeId,
        CancellationToken cancellationToken = default)
    {
        var showtimeResponse = await _movieApiClient.GetShowtimeByIdAsync(showtimeId, cancellationToken);
        if (!showtimeResponse.Success || showtimeResponse.Data is null)
        {
            throw new InvalidOperationException($"Showtime {showtimeId} not found");
        }

        var showtime = showtimeResponse.Data;
        var hallTask = _cinemaApiClient.GetHallByIdAsync(showtime.CinemaHallId, cancellationToken);
        var hallSeatsTask = _cinemaApiClient.GetHallSeatsAsync(showtime.CinemaHallId, cancellationToken);
        var redisSeatStatusesTask = _seatStatusService.GetCachedSeatStatusesAsync(showtimeId);
        var bookingsTask = _bookingRepository.GetByShowtimeIdAsync(showtimeId);

        await Task.WhenAll(hallTask, hallSeatsTask, redisSeatStatusesTask, bookingsTask);

        var hallResponse = await hallTask;
        var hallSeatsResponse = await hallSeatsTask;
        var redisSeatStatuses = await redisSeatStatusesTask;
        var bookings = await bookingsTask;

        if (!hallResponse.Success || hallResponse.Data is null)
        {
            throw new InvalidOperationException($"Cinema hall {showtime.CinemaHallId} not found");
        }

        var hall = hallResponse.Data;
        var hallSeats = ResolveHallSeats(hallSeatsResponse.Data, hall.Seats);
        var seats = BuildBaseSeatMap(hallSeats, showtime.Price);

        if (redisSeatStatuses.Count == 0 && seats.Count > 0)
        {
            await SeedRedisSeatMapAsync(showtimeId, hall.Id, hall.Name, seats, bookings);
            redisSeatStatuses = await _seatStatusService.GetCachedSeatStatusesAsync(showtimeId);
        }

        ApplyBookingState(seats, bookings);
        ApplyRedisLocks(seats, redisSeatStatuses);

        seats = seats
            .OrderBy(seat => seat.Row, StringComparer.OrdinalIgnoreCase)
            .ThenBy(seat => seat.Number)
            .ToList();

        return new SeatAvailabilityResponse
        {
            ShowtimeId = showtimeId,
            CinemaHallId = hall.Id,
            CinemaHallName = string.IsNullOrWhiteSpace(hall.Name)
                ? showtime.CinemaHallName ?? string.Empty
                : hall.Name,
            Seats = seats,
            Summary = BuildSummary(seats)
        };
    }

    private async Task SeedRedisSeatMapAsync(
        Guid showtimeId,
        Guid cinemaHallId,
        string cinemaHallName,
        IReadOnlyCollection<SeatStatusDto> sourceSeats,
        IReadOnlyCollection<BookingEntity> bookings)
    {
        _logger.LogInformation(
            "Redis seat map missing for showtime {ShowtimeId}; seeding from Cinema and Booking data",
            showtimeId);

        var seedSeats = sourceSeats
            .Select(seat => new SeatStatusDto
            {
                SeatId = seat.SeatId,
                Row = seat.Row,
                Number = seat.Number,
                Price = seat.Price,
                Status = SeatStatus.Available
            })
            .ToList();

        await _seatStatusService.InitializeSeatMapAsync(showtimeId, cinemaHallId, cinemaHallName, seedSeats);

        foreach (var booking in GetBookingsThatBlockSeats(bookings))
        {
            var seatIds = booking.GetSeatIds();
            if (seatIds.Count == 0)
            {
                continue;
            }

            await _seatStatusService.MarkSeatsAsBookedAsync(showtimeId, seatIds, booking.Id);
        }
    }

    private static List<CinemaSeatDto> ResolveHallSeats(
        IReadOnlyCollection<CinemaSeatDto>? hallSeats,
        IReadOnlyCollection<CinemaSeatDto>? hallDetailSeats)
    {
        if (hallSeats is { Count: > 0 })
        {
            return hallSeats.ToList();
        }

        return hallDetailSeats?.ToList() ?? [];
    }

    private static List<SeatStatusDto> BuildBaseSeatMap(
        IEnumerable<CinemaSeatDto> hallSeats,
        decimal showtimePrice)
    {
        return hallSeats
            .Select(seat => new SeatStatusDto
            {
                SeatId = seat.Id,
                Row = seat.Row,
                Number = seat.Number,
                Price = showtimePrice,
                Status = SeatStatus.Available
            })
            .ToList();
    }

    private static void ApplyBookingState(
        IReadOnlyCollection<SeatStatusDto> seats,
        IReadOnlyCollection<BookingEntity> bookings)
    {
        var seatMap = seats.ToDictionary(seat => seat.SeatId);
        var now = DateTime.UtcNow;

        foreach (var booking in GetBookingsThatBlockSeats(bookings))
        {
            var status = GetSeatStatusForBooking(booking, now);

            foreach (var seatId in booking.GetSeatIds())
            {
                if (!seatMap.TryGetValue(seatId, out var seat))
                {
                    continue;
                }

                if (seat.Status == SeatStatus.Booked)
                {
                    continue;
                }

                seat.Status = status;
                seat.LockedBy = status == SeatStatus.Locked ? booking.UserId : null;
                seat.LockedUntil = status == SeatStatus.Locked ? booking.ExpiresAt : null;
            }
        }
    }

    private static void ApplyRedisLocks(
        IReadOnlyCollection<SeatStatusDto> seats,
        IReadOnlyCollection<SeatStatusDto> redisSeats)
    {
        var redisSeatMap = redisSeats.ToDictionary(seat => seat.SeatId);
        var now = DateTime.UtcNow;

        foreach (var seat in seats)
        {
            if (seat.Status != SeatStatus.Available)
            {
                continue;
            }

            if (!redisSeatMap.TryGetValue(seat.SeatId, out var redisSeat))
            {
                continue;
            }

            if (redisSeat.Status != SeatStatus.Locked)
            {
                continue;
            }

            if (redisSeat.LockedUntil.HasValue && redisSeat.LockedUntil.Value <= now)
            {
                continue;
            }

            seat.Status = SeatStatus.Locked;
            seat.LockedBy = redisSeat.LockedBy;
            seat.LockedUntil = redisSeat.LockedUntil;
        }
    }

    private static IEnumerable<BookingEntity> GetBookingsThatBlockSeats(IEnumerable<BookingEntity> bookings)
    {
        var now = DateTime.UtcNow;

        return bookings.Where(booking =>
            booking.Status == BookingStatus.Confirmed ||
            booking.Status == BookingStatus.CheckedIn ||
            (booking.Status == BookingStatus.Pending &&
             (!booking.ExpiresAt.HasValue || booking.ExpiresAt.Value > now)));
    }

    private static SeatStatus GetSeatStatusForBooking(BookingEntity booking, DateTime now)
    {
        if (booking.Status == BookingStatus.Pending &&
            (!booking.ExpiresAt.HasValue || booking.ExpiresAt.Value > now))
        {
            return SeatStatus.Locked;
        }

        return SeatStatus.Booked;
    }

    private static SeatAvailabilitySummary BuildSummary(IReadOnlyCollection<SeatStatusDto> seats)
        => new()
        {
            TotalSeats = seats.Count,
            AvailableSeats = seats.Count(seat => seat.Status == SeatStatus.Available),
            LockedSeats = seats.Count(seat => seat.Status == SeatStatus.Locked),
            BookedSeats = seats.Count(seat => seat.Status == SeatStatus.Booked)
        };
}
