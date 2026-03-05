namespace Shared.Models.DataModels.DashboardDtos
{
    /// <summary>
    /// DTO phim ăn khách — thống kê theo doanh thu và lượng vé bán ra.
    /// </summary>
    public class TopMovieDto
    {
        /// <summary>ID phim.</summary>
        public Guid MovieId { get; set; }

        /// <summary>Tên phim.</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>Ảnh poster.</summary>
        public string? PosterUrl { get; set; }

        /// <summary>Tổng doanh thu từ phim này.</summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>Tổng số vé bán ra.</summary>
        public int TotalTicketsSold { get; set; }

        /// <summary>Số suất chiếu.</summary>
        public int ShowtimeCount { get; set; }
    }
}
