using BookingEntity = Booking.API.Domain.Entities.Booking;

namespace Booking.API.Application.Services;

public interface IDashboardInsightFactory
{
    Task<DashboardKpiDto> BuildKpiAsync(
        DashboardTimeContext context,
        List<BookingEntity> monthlyBookings,
        List<ShowtimeLookupDto> todayShowtimes,
        IReadOnlyDictionary<Guid, ShowtimeLookupDto> showtimeLookupMap,
        IReadOnlyDictionary<Guid, CinemaHallDto> hallLookupMap);

    DashboardRevenueChartDto BuildRevenueChart(
        DashboardTimeContext context,
        List<BookingEntity> monthlyBookings);

    Task<List<DashboardTopMovieDto>> BuildTopMoviesAsync(
        DashboardTimeContext context,
        List<BookingEntity> monthlyBookings,
        List<ShowtimeLookupDto> todayShowtimes,
        IReadOnlyDictionary<Guid, ShowtimeLookupDto> showtimeLookupMap,
        IReadOnlyDictionary<Guid, CinemaHallDto> hallLookupMap);

    Task<List<DashboardRecentActivityDto>> BuildRecentActivitiesAsync(
        List<BookingEntity> recentBookings,
        IReadOnlyDictionary<Guid, ShowtimeLookupDto> showtimeLookupMap);
}
