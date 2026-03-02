namespace Shared.Models.DataModels.DashboardDtos
{
    public class DashboardSummaryDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal TicketRevenue { get; set; }
        public decimal ConcessionRevenue { get; set; }
        public int TotalBookings { get; set; }
        public double OccupancyRate { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }
}
