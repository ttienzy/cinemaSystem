namespace Shared.Models.DataModels.DashboardDtos
{
    /// <summary>
    /// Revenue report DTO — contains total revenue and details by time period.
    /// </summary>
    public class RevenueReportDto
    {
        /// <summary>Total ticket revenue in period.</summary>
        public decimal TotalTicketRevenue { get; set; }

        /// <summary>Total concession revenue in period.</summary>
        public decimal TotalConcessionRevenue { get; set; }

        /// <summary>Grand total revenue (tickets + concessions).</summary>
        public decimal GrandTotal { get; set; }

        /// <summary>Start time period.</summary>
        public DateTime From { get; set; }

        /// <summary>End time period.</summary>
        public DateTime To { get; set; }

        /// <summary>Revenue details by group (day/week/month).</summary>
        public List<RevenueItemDto> Items { get; set; } = new();
    }

    /// <summary>
    /// A single revenue line in report — represents 1 day/week/month.
    /// </summary>
    public class RevenueItemDto
    {
        /// <summary>Time label (e.g. "2026-03-01", "Week 10", "March").</summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>Ticket revenue.</summary>
        public decimal TicketRevenue { get; set; }

        /// <summary>Concession revenue.</summary>
        public decimal ConcessionRevenue { get; set; }

        /// <summary>Total revenue.</summary>
        public decimal Total { get; set; }

        /// <summary>Number of bookings.</summary>
        public int BookingCount { get; set; }
    }
}
