using Booking.API.Application.DTOs.External;
using Booking.API.Application.DTOs.Responses;
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
    private readonly MovieApiClient _movieApiClient;
    private readonly CinemaApiClient _cinemaApiClient;
    private readonly IDashboardInsightFactory _dashboardInsightFactory;

    public DashboardService(
        BookingDbContext dbContext,
        MovieApiClient movieApiClient,
        CinemaApiClient cinemaApiClient,
        IDashboardInsightFactory dashboardInsightFactory)
    {
        _dbContext = dbContext;
        _movieApiClient = movieApiClient;
        _cinemaApiClient = cinemaApiClient;
        _dashboardInsightFactory = dashboardInsightFactory;
    }

    public async Task<ApiResponse<DashboardSummaryDto>> GetSummaryAsync(int utcOffsetMinutes)
    {
        var validationResponse = ValidateUtcOffset<DashboardSummaryDto>(utcOffsetMinutes);
        if (validationResponse != null)
        {
            return validationResponse;
        }

        var context = DashboardTimeContext.Create(utcOffsetMinutes);
        var summaryData = await LoadSummaryDataAsync(context);

        var kpi = await _dashboardInsightFactory.BuildKpiAsync(
            context,
            summaryData.MonthlyBookings,
            summaryData.TodayShowtimes,
            summaryData.ShowtimeLookupMap,
            summaryData.HallLookupMap);

        var response = new DashboardSummaryDto
        {
            Kpi = kpi,
            RevenueChart = _dashboardInsightFactory.BuildRevenueChart(context, summaryData.MonthlyBookings),
            TopMovies = await _dashboardInsightFactory.BuildTopMoviesAsync(
                context,
                summaryData.MonthlyBookings,
                summaryData.TodayShowtimes,
                summaryData.ShowtimeLookupMap,
                summaryData.HallLookupMap),
            RecentActivities = await _dashboardInsightFactory.BuildRecentActivitiesAsync(
                summaryData.RecentBookings,
                summaryData.ShowtimeLookupMap),
            GeneratedAtUtc = DateTime.UtcNow,
            UtcOffsetMinutes = utcOffsetMinutes
        };

        return ApiResponse<DashboardSummaryDto>.SuccessResponse(response);
    }

    public async Task<ApiResponse<DashboardKpiSnapshotDto>> GetKpiSnapshotAsync(int utcOffsetMinutes)
    {
        var validationResponse = ValidateUtcOffset<DashboardKpiSnapshotDto>(utcOffsetMinutes);
        if (validationResponse != null)
        {
            return validationResponse;
        }

        var context = DashboardTimeContext.Create(utcOffsetMinutes);
        var snapshotData = await LoadSnapshotDataAsync(context);

        var kpi = await _dashboardInsightFactory.BuildKpiAsync(
            context,
            snapshotData.MonthlyBookings,
            snapshotData.TodayShowtimes,
            snapshotData.ShowtimeLookupMap,
            snapshotData.HallLookupMap);

        return ApiResponse<DashboardKpiSnapshotDto>.SuccessResponse(kpi);
    }

    private async Task<DashboardSummaryData> LoadSummaryDataAsync(DashboardTimeContext context)
    {
        var monthlyBookings = await GetSuccessfulBookingsAsync(context.MonthlyStartUtc, context.TodayEndUtc);
        var recentBookings = await GetRecentSuccessfulBookingsAsync(10);
        var todayShowtimes = await _movieApiClient.GetShowtimesByRangeAsync(context.TodayStartUtc, context.TodayEndUtc);

        var showtimeLookupMap = await LoadShowtimeLookupMapAsync(monthlyBookings, recentBookings);
        var hallLookupMap = await LoadHallLookupMapAsync(todayShowtimes);

        return new DashboardSummaryData
        {
            MonthlyBookings = monthlyBookings,
            RecentBookings = recentBookings,
            TodayShowtimes = todayShowtimes,
            ShowtimeLookupMap = showtimeLookupMap,
            HallLookupMap = hallLookupMap
        };
    }

    private async Task<DashboardSnapshotData> LoadSnapshotDataAsync(DashboardTimeContext context)
    {
        var monthlyBookings = await GetSuccessfulBookingsAsync(context.MonthlyStartUtc, context.TodayEndUtc);
        var todayShowtimes = await _movieApiClient.GetShowtimesByRangeAsync(context.TodayStartUtc, context.TodayEndUtc);

        var showtimeLookupMap = await LoadShowtimeLookupMapAsync(monthlyBookings);
        var hallLookupMap = await LoadHallLookupMapAsync(todayShowtimes);

        return new DashboardSnapshotData
        {
            MonthlyBookings = monthlyBookings,
            TodayShowtimes = todayShowtimes,
            ShowtimeLookupMap = showtimeLookupMap,
            HallLookupMap = hallLookupMap
        };
    }

    private async Task<IReadOnlyDictionary<Guid, ShowtimeLookupDto>> LoadShowtimeLookupMapAsync(
        IEnumerable<BookingEntity> primaryBookings,
        IEnumerable<BookingEntity>? secondaryBookings = null)
    {
        var showtimeIds = primaryBookings
            .Select(booking => booking.ShowtimeId)
            .Concat(secondaryBookings?.Select(booking => booking.ShowtimeId) ?? [])
            .Distinct()
            .ToList();

        if (showtimeIds.Count == 0)
        {
            return new Dictionary<Guid, ShowtimeLookupDto>();
        }

        var lookups = await _movieApiClient.GetShowtimeLookupsByIdsAsync(showtimeIds);
        return lookups.ToDictionary(item => item.ShowtimeId);
    }

    private async Task<IReadOnlyDictionary<Guid, CinemaHallDto>> LoadHallLookupMapAsync(
        IEnumerable<ShowtimeLookupDto> todayShowtimes)
    {
        var hallIds = todayShowtimes
            .Select(showtime => showtime.CinemaHallId)
            .Distinct()
            .ToList();

        if (hallIds.Count == 0)
        {
            return new Dictionary<Guid, CinemaHallDto>();
        }

        var halls = await _cinemaApiClient.GetCinemaHallsByIdsAsync(hallIds);
        return halls.ToDictionary(item => item.Id);
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

    private static ApiResponse<T>? ValidateUtcOffset<T>(int utcOffsetMinutes)
    {
        if (DashboardTimeContext.IsValidUtcOffset(utcOffsetMinutes))
        {
            return null;
        }

        var value = DashboardException.INVALID_UTC_OFFSET;
        return ApiResponse<T>.ValidationErrorResponse(
            DashboardException.VALIDATION_FAILED,
            [new ErrorDetail(value.Code, value.Message, value.Field)]);
    }

    private sealed class DashboardSummaryData
    {
        public List<BookingEntity> MonthlyBookings { get; init; } = [];
        public List<BookingEntity> RecentBookings { get; init; } = [];
        public List<ShowtimeLookupDto> TodayShowtimes { get; init; } = [];
        public IReadOnlyDictionary<Guid, ShowtimeLookupDto> ShowtimeLookupMap { get; init; } =
            new Dictionary<Guid, ShowtimeLookupDto>();
        public IReadOnlyDictionary<Guid, CinemaHallDto> HallLookupMap { get; init; } =
            new Dictionary<Guid, CinemaHallDto>();
    }

    private sealed class DashboardSnapshotData
    {
        public List<BookingEntity> MonthlyBookings { get; init; } = [];
        public List<ShowtimeLookupDto> TodayShowtimes { get; init; } = [];
        public IReadOnlyDictionary<Guid, ShowtimeLookupDto> ShowtimeLookupMap { get; init; } =
            new Dictionary<Guid, ShowtimeLookupDto>();
        public IReadOnlyDictionary<Guid, CinemaHallDto> HallLookupMap { get; init; } =
            new Dictionary<Guid, CinemaHallDto>();
    }
}
