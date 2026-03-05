namespace Shared.Models.DataModels.BookingDtos
{
    public class PosDailySummaryDto
    {
        public Guid ShiftId { get; set; }
        public DateTime Date { get; set; }
        public string ShiftName { get; set; } = string.Empty;

        // Ticket sales
        public int TotalTicketsSold { get; set; }
        public decimal TicketRevenue { get; set; }

        // Concession sales
        public int TotalConcessionItems { get; set; }
        public decimal ConcessionRevenue { get; set; }

        // Booking stats
        public int TotalBookings { get; set; }
        public int CancelledBookings { get; set; }
        public int RefundedBookings { get; set; }

        // Totals
        public decimal TotalRevenue { get; set; }

        // Timestamps
        public DateTime GeneratedAt { get; set; }
    }
}
