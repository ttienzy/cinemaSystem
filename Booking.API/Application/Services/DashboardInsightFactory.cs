using BookingEntity = Booking.API.Domain.Entities.Booking;

namespace Booking.API.Application.Services;

public class DashboardInsightFactory : IDashboardInsightFactory
{
    private readonly IBookingRepository _bookingRepository;
    private readonly PaymentApiClient _paymentApiClient;

    public DashboardInsightFactory(
        IBookingRepository bookingRepository,
        PaymentApiClient paymentApiClient)
    {
        _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
        _paymentApiClient = paymentApiClient ?? throw new ArgumentNullException(nameof(paymentApiClient));
    }

    public async Task<DashboardKpiDto> BuildKpiAsync(
        DashboardTimeContext context,
        List<BookingEntity> monthlyBookings,
        List<ShowtimeLookupDto> todayShowtimes,
        IReadOnlyDictionary<Guid, ShowtimeLookupDto> showtimeLookupMap,
        IReadOnlyDictionary<Guid, CinemaHallDto> hallLookupMap)
    {
        var todayBookings = monthlyBookings
            .Where(booking => booking.BookingDate >= context.TodayStartUtc && booking.BookingDate < context.TodayEndUtc)
            .ToList();

        return new DashboardKpiDto
        {
            TodayRevenue = todayBookings.Sum(booking => booking.TotalPrice),
            TodayTicketsSold = todayBookings.Sum(booking => booking.BookingSeats.Count),
            OccupancyRate = await CalculateOccupancyRateAsync(todayShowtimes, hallLookupMap),
            TodayShowtimesCount = todayShowtimes.Count,
            ShowingMoviesCount = todayShowtimes
                .Select(showtime => showtime.MovieId)
                .Distinct()
                .Count(),
            HotMovie = BuildHotMovie(context, monthlyBookings, showtimeLookupMap),
            GeneratedAtUtc = DateTime.UtcNow,
            UtcOffsetMinutes = context.UtcOffsetMinutes
        };
    }

    public DashboardRevenueChartDto BuildRevenueChart(
        DashboardTimeContext context,
        List<BookingEntity> monthlyBookings)
    {
        return new DashboardRevenueChartDto
        {
            Weekly = BuildRevenueSeries(context, monthlyBookings, 7),
            Monthly = BuildRevenueSeries(context, monthlyBookings, 30)
        };
    }

    public async Task<List<DashboardTopMovieDto>> BuildTopMoviesAsync(
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

        var bookedSeatCounts = await _bookingRepository.GetBookedSeatCountsByShowtimeIdsAsync(
            todayShowtimes.Select(showtime => showtime.ShowtimeId));
        var movieOccupancyMap = BuildMovieOccupancyMap(todayShowtimes, hallLookupMap, bookedSeatCounts);

        return topMovieBookings
            .GroupBy(booking => showtimeLookupMap[booking.ShowtimeId].MovieId)
            .Select(group =>
            {
                var sampleShowtime = showtimeLookupMap[group.First().ShowtimeId];
                var currentWindowCount = group.Count(booking => booking.BookingDate >= context.CurrentTrendWindowStartUtc);
                var previousWindowCount = group.Count(booking =>
                    booking.BookingDate >= context.PreviousTrendWindowStartUtc &&
                    booking.BookingDate < context.CurrentTrendWindowStartUtc);

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

    public async Task<List<DashboardRecentActivityDto>> BuildRecentActivitiesAsync(
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
                    Status = "Completed",
                    OccurredAtUtc = payment?.CompletedAt ?? booking.UpdatedAt ?? booking.BookingDate
                };
            })
            .OrderByDescending(item => item.OccurredAtUtc)
            .Take(10)
            .ToList();
    }

    private static DashboardHotMovieDto? BuildHotMovie(
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

    private static List<DashboardRevenuePointDto> BuildRevenueSeries(
        DashboardTimeContext context,
        List<BookingEntity> bookings,
        int days)
    {
        var seriesStartLocal = context.GetRevenueSeriesStartLocal(days);
        var bookingsByLocalDate = bookings
            .GroupBy(booking => context.ConvertUtcToLocalDate(booking.BookingDate))
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

    private async Task<decimal> CalculateOccupancyRateAsync(
        List<ShowtimeLookupDto> todayShowtimes,
        IReadOnlyDictionary<Guid, CinemaHallDto> hallLookupMap)
    {
        if (todayShowtimes.Count == 0)
        {
            return 0;
        }

        var bookedSeatCounts = await _bookingRepository.GetBookedSeatCountsByShowtimeIdsAsync(
            todayShowtimes.Select(showtime => showtime.ShowtimeId));

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

    private static Dictionary<Guid, decimal> BuildMovieOccupancyMap(
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
}
