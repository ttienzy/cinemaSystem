namespace Booking.API.Application.DTOs.Responses;

public class DashboardSummaryDto
{
    public DashboardKpiDto Kpi { get; set; } = new();
    public DashboardRevenueChartDto RevenueChart { get; set; } = new();
    public List<DashboardTopMovieDto> TopMovies { get; set; } = new();
    public List<DashboardRecentActivityDto> RecentActivities { get; set; } = new();
    public DateTime GeneratedAtUtc { get; set; }
    public int UtcOffsetMinutes { get; set; }
}

public class DashboardKpiSnapshotDto
{
    public decimal TodayRevenue { get; set; }
    public int TodayTicketsSold { get; set; }
    public decimal OccupancyRate { get; set; }
    public int TodayShowtimesCount { get; set; }
    public int ShowingMoviesCount { get; set; }
    public DashboardHotMovieDto? HotMovie { get; set; }
    public DateTime GeneratedAtUtc { get; set; }
    public int UtcOffsetMinutes { get; set; }
}

public class DashboardKpiDto : DashboardKpiSnapshotDto
{
}

public class DashboardRevenueChartDto
{
    public List<DashboardRevenuePointDto> Weekly { get; set; } = new();
    public List<DashboardRevenuePointDto> Monthly { get; set; } = new();
}

public class DashboardRevenuePointDto
{
    public DateTime Date { get; set; }
    public string Label { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int TicketsSold { get; set; }
    public int BookingsCount { get; set; }
}

public class DashboardHotMovieDto
{
    public Guid MovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? PosterUrl { get; set; }
    public int BookingsCount { get; set; }
    public int TicketsSold { get; set; }
    public decimal Revenue { get; set; }
}

public class DashboardTopMovieDto
{
    public Guid MovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? PosterUrl { get; set; }
    public int Rank { get; set; }
    public int BookingsCount { get; set; }
    public int TicketsSold { get; set; }
    public decimal Revenue { get; set; }
    public decimal OccupancyRate { get; set; }
    public string TrendDirection { get; set; } = "stable";
    public DateTime? LastBookingAtUtc { get; set; }
}

public class DashboardRecentActivityDto
{
    public Guid BookingId { get; set; }
    public Guid ShowtimeId { get; set; }
    public Guid MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int SeatsCount { get; set; }
    public string Status { get; set; } = "Completed";
    public DateTime OccurredAtUtc { get; set; }
}
