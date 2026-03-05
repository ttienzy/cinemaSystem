namespace Shared.Models.DataModels.DashboardDtos
{
    /// <summary>
    /// DTO báo cáo doanh thu — chứa tổng doanh thu và chi tiết theo khoảng thời gian.
    /// </summary>
    public class RevenueReportDto
    {
        /// <summary>Tổng doanh thu vé trong khoảng.</summary>
        public decimal TotalTicketRevenue { get; set; }

        /// <summary>Tổng doanh thu bắp nước trong khoảng.</summary>
        public decimal TotalConcessionRevenue { get; set; }

        /// <summary>Tổng doanh thu (vé + bắp nước).</summary>
        public decimal GrandTotal { get; set; }

        /// <summary>Khoảng thời gian bắt đầu.</summary>
        public DateTime From { get; set; }

        /// <summary>Khoảng thời gian kết thúc.</summary>
        public DateTime To { get; set; }

        /// <summary>Chi tiết doanh thu theo từng nhóm (ngày/tuần/tháng).</summary>
        public List<RevenueItemDto> Items { get; set; } = new();
    }

    /// <summary>
    /// Một dòng doanh thu trong báo cáo — đại diện 1 ngày/tuần/tháng.
    /// </summary>
    public class RevenueItemDto
    {
        /// <summary>Nhãn thời gian (ví dụ: "2026-03-01", "Tuần 10", "Tháng 3").</summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>Doanh thu vé.</summary>
        public decimal TicketRevenue { get; set; }

        /// <summary>Doanh thu bắp nước.</summary>
        public decimal ConcessionRevenue { get; set; }

        /// <summary>Tổng doanh thu.</summary>
        public decimal Total { get; set; }

        /// <summary>Số booking.</summary>
        public int BookingCount { get; set; }
    }
}
