using Booking.API.Client;
using Booking.API.Models;
using BookingEntity = Booking.API.Entities.Booking;

namespace Booking.API.Services;

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
