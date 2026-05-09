using Booking.API.Application.DTOs.External;
using Booking.API.Application.DTOs.Responses;
using Booking.API.Domain.Entities;
using Cinema.Shared.Models;
using Microsoft.EntityFrameworkCore;
using BookingEntity = Booking.API.Domain.Entities.Booking;

namespace Booking.API.Application.Services;

public class DashboardService : IDashboardService
{
    private static readonly BookingStatus[] SuccessfulBookingStatuses =
    [
        BookingStatus.Confirmed,
        BookingStatus.CheckedIn
    ];

    private readonly BookingDbContext _dbContext;
    private readonly IBookingRepository _bookingRepository;
    private readonly MovieApiClient _movieApiClient;
    private readonly CinemaApiClient _cinemaApiClient;
    private readonly PaymentApiClient _paymentApiClient;

    public DashboardService(
        BookingDbContext dbContext,
        IBookingRepository bookingRepository,
        MovieApiClient movieApiClient,
        CinemaApiClient cinemaApiClient,
        PaymentApiClient paymentApiClient)
    {
        _dbContext = dbContext;
        _bookingRepository = bookingRepository;
        _movieApiClient = movieApiClient;
        _cinemaApiClient = cinemaApiClient;
        _paymentApiClient = paymentApiClient;
    }

    public async Task<ApiResponse<DashboardSummaryDto>> GetSummaryAsync(int utcOffsetMinutes)
    {
        if (!IsValidUtcOffset(utcOffsetMinutes))
        {
            return InvalidUtcOffsetResponse<DashboardSummaryDto>();
        }

        var context = CreateDashboardTimeContext(utcOffsetMinutes);

        // DbContext is NOT thread-safe — queries must run sequentially.
        // Only HTTP calls (no shared DbContext) can be parallelized safely.
        var monthlyBookings = await GetSuccessfulBookingsAsync(context.MonthlyStartUtc, context.TodayEndUtc);
        var recentBookings = await GetRecentSuccessfulBookingsAsync(10);
        var todayShowtimes = await _movieApiClient.GetShowtimesByRangeAsync(context.TodayStartUtc, context.TodayEndUtc);

        var lookupShowtimeIds = monthlyBookings
            .Select(booking => booking.ShowtimeId)
            .Concat(recentBookings.Select(booking => booking.ShowtimeId))
            .Distinct()
            .ToList();

        var lookupHallIds = todayShowtimes
            .Select(showtime => showtime.CinemaHallId)
            .Distinct()
            .ToList();

        var showtimeLookupTask = _movieApiClient.GetShowtimeLookupsByIdsAsync(lookupShowtimeIds);
        var hallLookupTask = _cinemaApiClient.GetCinemaHallsByIdsAsync(lookupHallIds);
        await Task.WhenAll(showtimeLookupTask, hallLookupTask);

        var showtimeLookupMap = showtimeLookupTask.Result.ToDictionary(item => item.ShowtimeId);
        var hallLookupMap = hallLookupTask.Result.ToDictionary(item => item.Id);

        var kpi = await BuildKpiAsync(context, monthlyBookings, todayShowtimes, showtimeLookupMap, hallLookupMap);
        var revenueChart = BuildRevenueChart(context, monthlyBookings);
        var topMovies = await BuildTopMoviesAsync(context, monthlyBookings, todayShowtimes, showtimeLookupMap, hallLookupMap);
        var recentActivities = await BuildRecentActivitiesAsync(recentBookings, showtimeLookupMap);

        var response = new DashboardSummaryDto
        {
            Kpi = kpi,
            RevenueChart = revenueChart,
            TopMovies = topMovies,
            RecentActivities = recentActivities,
            GeneratedAtUtc = DateTime.UtcNow,
            UtcOffsetMinutes = utcOffsetMinutes
        };

        return ApiResponse<DashboardSummaryDto>.SuccessResponse(response);
    }

    public async Task<ApiResponse<DashboardKpiSnapshotDto>> GetKpiSnapshotAsync(int utcOffsetMinutes)
    {
        if (!IsValidUtcOffset(utcOffsetMinutes))
        {
            return InvalidUtcOffsetResponse<DashboardKpiSnapshotDto>();
        }

        var context = CreateDashboardTimeContext(utcOffsetMinutes);

        var monthlyBookings = await GetSuccessfulBookingsAsync(context.MonthlyStartUtc, context.TodayEndUtc);
        var todayShowtimes = await _movieApiClient.GetShowtimesByRangeAsync(context.TodayStartUtc, context.TodayEndUtc);

        var lookupShowtimeIds = monthlyBookings
            .Select(booking => booking.ShowtimeId)
            .Distinct()
            .ToList();

        var lookupHallIds = todayShowtimes
            .Select(showtime => showtime.CinemaHallId)
            .Distinct()
            .ToList();

        var showtimeLookupTask = _movieApiClient.GetShowtimeLookupsByIdsAsync(lookupShowtimeIds);
        var hallLookupTask = _cinemaApiClient.GetCinemaHallsByIdsAsync(lookupHallIds);
        await Task.WhenAll(showtimeLookupTask, hallLookupTask);

        var showtimeLookupMap = showtimeLookupTask.Result.ToDictionary(item => item.ShowtimeId);
        var hallLookupMap = hallLookupTask.Result.ToDictionary(item => item.Id);

        var kpi = await BuildKpiAsync(context, monthlyBookings, todayShowtimes, showtimeLookupMap, hallLookupMap);

        return ApiResponse<DashboardKpiSnapshotDto>.SuccessResponse(kpi);
    }

    private async Task<List<BookingEntity>> GetSuccessfulBookingsAsync(DateTime fromUtc, DateTime toUtc)
    {
        return await _dbContext.Bookings
            .AsNoTracking()
            .Include(booking => booking.BookingSeats)
            .Where(booking =>
                SuccessfulBookingStatuses.Contains(booking.Status) &&
                booking.BookingDate >= fromUtc &&
                booking.BookingDate < toUtc)
            .ToListAsync();
    }

    private async Task<List<BookingEntity>> GetRecentSuccessfulBookingsAsync(int take)
    {
        return await _dbContext.Bookings
            .AsNoTracking()
            .Include(booking => booking.BookingSeats)
            .Where(booking => SuccessfulBookingStatuses.Contains(booking.Status))
            .OrderByDescending(booking => booking.UpdatedAt ?? booking.BookingDate)
            .Take(take)
            .ToListAsync();
    }

    private async Task<DashboardKpiDto> BuildKpiAsync(
        DashboardTimeContext context,
        List<BookingEntity> monthlyBookings,
        List<ShowtimeLookupDto> todayShowtimes,
        IReadOnlyDictionary<Guid, ShowtimeLookupDto> showtimeLookupMap,
        IReadOnlyDictionary<Guid, CinemaHallDto> hallLookupMap)
    {
        var todayBookings = monthlyBookings
            .Where(booking => booking.BookingDate >= context.TodayStartUtc && booking.BookingDate < context.TodayEndUtc)
            .ToList();

        var occupancyRate = await CalculateOccupancyRateAsync(todayShowtimes, hallLookupMap);
        var hotMovie = BuildHotMovie(context, monthlyBookings, showtimeLookupMap);

        return new DashboardKpiDto
        {
            TodayRevenue = todayBookings.Sum(booking => booking.TotalPrice),
            TodayTicketsSold = todayBookings.Sum(booking => booking.BookingSeats.Count),
            OccupancyRate = occupancyRate,
            TodayShowtimesCount = todayShowtimes.Count,
            ShowingMoviesCount = todayShowtimes
                .Select(showtime => showtime.MovieId)
                .Distinct()
                .Count(),
            HotMovie = hotMovie,
            GeneratedAtUtc = DateTime.UtcNow,
            UtcOffsetMinutes = context.UtcOffsetMinutes
        };
    }

    private DashboardRevenueChartDto BuildRevenueChart(DashboardTimeContext context, List<BookingEntity> monthlyBookings)
    {
        return new DashboardRevenueChartDto
        {
            Weekly = BuildRevenueSeries(context, monthlyBookings, 7),
            Monthly = BuildRevenueSeries(context, monthlyBookings, 30)
        };
    }

    private List<DashboardRevenuePointDto> BuildRevenueSeries(DashboardTimeContext context, List<BookingEntity> bookings, int days)
    {
        var seriesStartLocal = context.LocalTodayStart.AddDays(-(days - 1));
        var bookingsByLocalDate = bookings
            .GroupBy(booking => ConvertUtcToLocalDate(booking.BookingDate, context.UtcOffsetMinutes))
            .ToDictionary(group => group.Key, group => group.ToList());

        return Enumerable.Range(0, days)
            .Select(index =>
            {
                var localDate = seriesStartLocal.AddDays(index);
                var dayBookings = bookingsByLocalDate.GetValueOrDefault(localDate.Date) ?? [];

                return new DashboardRevenuePointDto
                {
                    Date = localDate,
                    Label = localDate.ToString("dd/MM"),
                    Revenue = dayBookings.Sum(booking => booking.TotalPrice),
                    TicketsSold = dayBookings.Sum(booking => booking.BookingSeats.Count),
                    BookingsCount = dayBookings.Count
                };
            })
            .ToList();
    }

    private async Task<List<DashboardTopMovieDto>> BuildTopMoviesAsync(
        DashboardTimeContext context,
        List<BookingEntity> monthlyBookings,
        List<ShowtimeLookupDto> todayShowtimes,
        IReadOnlyDictionary<Guid, ShowtimeLookupDto> showtimeLookupMap,
        IReadOnlyDictionary<Guid, CinemaHallDto> hallLookupMap)
    {
        var topMovieBookings = monthlyBookings
            .Where(booking => booking.BookingDate >= context.TopMoviesStartUtc)
            .Where(booking => showtimeLookupMap.ContainsKey(booking.ShowtimeId))
            .ToList();

        var currentTrendWindowStart = context.NowUtc.AddHours(-24);
        var previousTrendWindowStart = currentTrendWindowStart.AddHours(-24);

        var bookedSeatCounts = await _bookingRepository.GetBookedSeatCountsByShowtimeIdsAsync(
            todayShowtimes.Select(showtime => showtime.ShowtimeId));
        var movieOccupancyMap = BuildMovieOccupancyMap(todayShowtimes, hallLookupMap, bookedSeatCounts);

        return topMovieBookings
            .GroupBy(booking => showtimeLookupMap[booking.ShowtimeId].MovieId)
            .Select(group =>
            {
                var sampleShowtime = showtimeLookupMap[group.First().ShowtimeId];
                var currentWindowCount = group.Count(booking => booking.BookingDate >= currentTrendWindowStart);
                var previousWindowCount = group.Count(booking => booking.BookingDate >= previousTrendWindowStart && booking.BookingDate < currentTrendWindowStart);

                return new DashboardTopMovieDto
                {
                    MovieId = sampleShowtime.MovieId,
                    Title = sampleShowtime.MovieTitle,
                    PosterUrl = sampleShowtime.PosterUrl,
                    BookingsCount = group.Count(),
                    TicketsSold = group.Sum(booking => booking.BookingSeats.Count),
                    Revenue = group.Sum(booking => booking.TotalPrice),
                    OccupancyRate = movieOccupancyMap.GetValueOrDefault(sampleShowtime.MovieId),
                    TrendDirection = currentWindowCount == previousWindowCount
                        ? "stable"
                        : currentWindowCount > previousWindowCount ? "up" : "down",
                    LastBookingAtUtc = group.Max(booking => (DateTime?)(booking.UpdatedAt ?? booking.BookingDate))
                };
            })
            .OrderByDescending(item => item.Revenue)
            .ThenByDescending(item => item.TicketsSold)
            .Take(5)
            .Select((item, index) =>
            {
                item.Rank = index + 1;
                return item;
            })
            .ToList();
    }

    private async Task<List<DashboardRecentActivityDto>> BuildRecentActivitiesAsync(
        List<BookingEntity> recentBookings,
        IReadOnlyDictionary<Guid, ShowtimeLookupDto> showtimeLookupMap)
    {
        var paymentLookupTasks = recentBookings.ToDictionary(
            booking => booking.Id,
            booking => _paymentApiClient.GetPaymentByBookingIdAsync(booking.Id));

        await Task.WhenAll(paymentLookupTasks.Values);

        return recentBookings
            .Where(booking => showtimeLookupMap.ContainsKey(booking.ShowtimeId))
            .Select(booking =>
            {
                var showtime = showtimeLookupMap[booking.ShowtimeId];
                var payment = paymentLookupTasks[booking.Id].Result;

                return new DashboardRecentActivityDto
                {
                    BookingId = booking.Id,
                    ShowtimeId = booking.ShowtimeId,
                    MovieId = showtime.MovieId,
                    MovieTitle = showtime.MovieTitle,
                    CustomerName = payment?.CustomerName ?? booking.UserId,
                    Amount = payment is null ? booking.TotalPrice : Convert.ToDecimal(payment.Amount),
                    SeatsCount = booking.BookingSeats.Count,
                    Status = payment?.Status.ToString() ?? "Completed",
                    OccurredAtUtc = payment?.CompletedAt ?? booking.UpdatedAt ?? booking.BookingDate
                };
            })
            .OrderByDescending(item => item.OccurredAtUtc)
            .Take(10)
            .ToList();
    }

    private DashboardHotMovieDto? BuildHotMovie(
        DashboardTimeContext context,
        List<BookingEntity> monthlyBookings,
        IReadOnlyDictionary<Guid, ShowtimeLookupDto> showtimeLookupMap)
    {
        var hotMovieBookings = monthlyBookings
            .Where(booking => booking.BookingDate >= context.HotMovieStartUtc)
            .Where(booking => showtimeLookupMap.ContainsKey(booking.ShowtimeId))
            .ToList();

        return hotMovieBookings
            .GroupBy(booking => showtimeLookupMap[booking.ShowtimeId].MovieId)
            .Select(group =>
            {
                var sampleShowtime = showtimeLookupMap[group.First().ShowtimeId];
                return new DashboardHotMovieDto
                {
                    MovieId = sampleShowtime.MovieId,
                    Title = sampleShowtime.MovieTitle,
                    PosterUrl = sampleShowtime.PosterUrl,
                    BookingsCount = group.Count(),
                    TicketsSold = group.Sum(booking => booking.BookingSeats.Count),
                    Revenue = group.Sum(booking => booking.TotalPrice)
                };
            })
            .OrderByDescending(item => item.TicketsSold)
            .ThenByDescending(item => item.Revenue)
            .FirstOrDefault();
    }

    private async Task<decimal> CalculateOccupancyRateAsync(
        List<ShowtimeLookupDto> todayShowtimes,
        IReadOnlyDictionary<Guid, CinemaHallDto> hallLookupMap)
    {
        if (todayShowtimes.Count == 0)
        {
            return 0;
        }

        var showtimeIds = todayShowtimes
            .Select(showtime => showtime.ShowtimeId)
            .ToList();

        var bookedSeatCounts = await _bookingRepository.GetBookedSeatCountsByShowtimeIdsAsync(showtimeIds);

        var totalSeats = 0;
        var bookedSeats = 0;

        foreach (var showtime in todayShowtimes)
        {
            if (!hallLookupMap.TryGetValue(showtime.CinemaHallId, out var hall))
            {
                continue;
            }

            totalSeats += hall.TotalSeats;
            bookedSeats += bookedSeatCounts.GetValueOrDefault(showtime.ShowtimeId);
        }

        if (totalSeats == 0)
        {
            return 0;
        }

        return Math.Round((decimal)bookedSeats * 100 / totalSeats, 2);
    }

    private Dictionary<Guid, decimal> BuildMovieOccupancyMap(
        List<ShowtimeLookupDto> todayShowtimes,
        IReadOnlyDictionary<Guid, CinemaHallDto> hallLookupMap,
        IReadOnlyDictionary<Guid, int> bookedSeatCounts)
    {
        return todayShowtimes
            .Where(showtime => hallLookupMap.ContainsKey(showtime.CinemaHallId))
            .GroupBy(showtime => showtime.MovieId)
            .ToDictionary(
                group => group.Key,
                group =>
                {
                    var totalSeats = group.Sum(showtime => hallLookupMap[showtime.CinemaHallId].TotalSeats);
                    if (totalSeats == 0)
                    {
                        return 0m;
                    }

                    var bookedSeats = group.Sum(showtime => bookedSeatCounts.GetValueOrDefault(showtime.ShowtimeId));
                    return Math.Round((decimal)bookedSeats * 100 / totalSeats, 2);
                });
    }

    private static DateTime ConvertUtcToLocalDate(DateTime utcDateTime, int utcOffsetMinutes)
    {
        return utcDateTime.AddMinutes(utcOffsetMinutes).Date;
    }

    private static bool IsValidUtcOffset(int utcOffsetMinutes)
    {
        return utcOffsetMinutes is >= -720 and <= 840;
    }

    private static ApiResponse<T> InvalidUtcOffsetResponse<T>()
    {
        return ApiResponse<T>.ValidationErrorResponse(
            "Validation failed",
            [new ErrorDetail("INVALID_UTC_OFFSET", "utcOffsetMinutes must be between -720 and 840.", "utcOffsetMinutes")]);
    }

    private static DashboardTimeContext CreateDashboardTimeContext(int utcOffsetMinutes)
    {
        var nowUtc = DateTime.UtcNow;
        var localNow = nowUtc.AddMinutes(utcOffsetMinutes);
        var localTodayStart = localNow.Date;

        return new DashboardTimeContext
        {
            UtcOffsetMinutes = utcOffsetMinutes,
            NowUtc = nowUtc,
            LocalTodayStart = localTodayStart,
            TodayStartUtc = localTodayStart.AddMinutes(-utcOffsetMinutes),
            TodayEndUtc = localTodayStart.AddDays(1).AddMinutes(-utcOffsetMinutes),
            MonthlyStartUtc = localTodayStart.AddDays(-29).AddMinutes(-utcOffsetMinutes),
            TopMoviesStartUtc = localTodayStart.AddDays(-6).AddMinutes(-utcOffsetMinutes),
            HotMovieStartUtc = nowUtc.AddHours(-24)
        };
    }

    private sealed class DashboardTimeContext
    {
        public int UtcOffsetMinutes { get; init; }
        public DateTime NowUtc { get; init; }
        public DateTime LocalTodayStart { get; init; }
        public DateTime TodayStartUtc { get; init; }
        public DateTime TodayEndUtc { get; init; }
        public DateTime MonthlyStartUtc { get; init; }
        public DateTime TopMoviesStartUtc { get; init; }
        public DateTime HotMovieStartUtc { get; init; }
    }
}
