namespace Booking.API.Application.Models;

public sealed class DashboardTimeContext
{
    public int UtcOffsetMinutes { get; init; }
    public DateTime NowUtc { get; init; }
    public DateTime LocalTodayStart { get; init; }
    public DateTime TodayStartUtc { get; init; }
    public DateTime TodayEndUtc { get; init; }
    public DateTime MonthlyStartUtc { get; init; }
    public DateTime TopMoviesStartUtc { get; init; }
    public DateTime HotMovieStartUtc { get; init; }
    public DateTime CurrentTrendWindowStartUtc { get; init; }
    public DateTime PreviousTrendWindowStartUtc { get; init; }

    public static bool IsValidUtcOffset(int utcOffsetMinutes)
    {
        return utcOffsetMinutes is >= -720 and <= 840;
    }

    public static DashboardTimeContext Create(int utcOffsetMinutes)
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
            HotMovieStartUtc = nowUtc.AddHours(-24),
            CurrentTrendWindowStartUtc = nowUtc.AddHours(-24),
            PreviousTrendWindowStartUtc = nowUtc.AddHours(-48)
        };
    }

    public DateTime ConvertUtcToLocalDate(DateTime utcDateTime)
    {
        return utcDateTime.AddMinutes(UtcOffsetMinutes).Date;
    }

    public DateTime GetRevenueSeriesStartLocal(int days)
    {
        return LocalTodayStart.AddDays(-(days - 1));
    }
}
