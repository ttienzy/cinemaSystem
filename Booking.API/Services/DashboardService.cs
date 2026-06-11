using Booking.API.Client;
using Booking.API.Data;
using Booking.API.Entities;
using Booking.API.Exceptions;
using Booking.API.Models;
using Microsoft.EntityFrameworkCore;
using ICinemaApiClient = Cinema.API.Client.Client.ICinemaApiClient;
using IMovieApiClient = Movie.API.Client.Client.IMovieApiClient;
using DomainBookingStatus = Booking.API.Entities.BookingStatus;
using BookingEntity = Booking.API.Entities.Booking;
using Booking.API.Mappers;

namespace Booking.API.Services;

public class DashboardService : IDashboardService
{
    private static readonly DomainBookingStatus[] SuccessfulBookingStatuses =
    [
        DomainBookingStatus.Confirmed,
        DomainBookingStatus.CheckedIn
    ];

    private readonly BookingDbContext _dbContext;
    private readonly IMovieApiClient _movieApiClient;
    private readonly ICinemaApiClient _cinemaApiClient;
    private readonly IDashboardInsightFactory _dashboardInsightFactory;

    public DashboardService(
        BookingDbContext dbContext,
        IMovieApiClient movieApiClient,
        ICinemaApiClient cinemaApiClient,
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
        var todayShowtimes = await GetShowtimesByRangeAsync(context.TodayStartUtc, context.TodayEndUtc);

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
        var todayShowtimes = await GetShowtimesByRangeAsync(context.TodayStartUtc, context.TodayEndUtc);

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

        var response = await _movieApiClient.LookupShowtimesAsync(
            new Movie.API.Client.ShowtimeLookupRequest { ShowtimeIds = showtimeIds });

        if (!response.Success || response.Data is null)
        {
            return new Dictionary<Guid, ShowtimeLookupDto>();
        }

        return response.Data
            .Select(ExternalClientDtoMapper.ToBookingShowtimeLookup)
            .ToDictionary(item => item.ShowtimeId);
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

        var response = await _cinemaApiClient.LookupHallsAsync(
            new Cinema.API.Client.CinemaHallLookupRequest { CinemaHallIds = hallIds });

        if (!response.Success || response.Data is null)
        {
            return new Dictionary<Guid, CinemaHallDto>();
        }

        return response.Data
            .Select(ExternalClientDtoMapper.ToBookingCinemaHall)
            .ToDictionary(item => item.Id);
    }

    private async Task<List<ShowtimeLookupDto>> GetShowtimesByRangeAsync(DateTime fromUtc, DateTime toUtc)
    {
        var response = await _movieApiClient.GetShowtimesByRangeAsync(fromUtc, toUtc);
        return response.Success && response.Data is not null
            ? response.Data.Select(ExternalClientDtoMapper.ToBookingShowtimeLookup).ToList()
            : [];
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
