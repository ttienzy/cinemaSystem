using Domain.Entities.ShowtimeAggregate.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.ShowtimeDtos
{
    public class ShowtimePerformanceDto
    {
        public Guid ShowtimeId { get; set; }

        // Thông tin phim
        public string Title { get; set; } = string.Empty;

        // Thông tin phòng chiếu
        public string ScreenName { get; set; } = string.Empty;
        public string ScreenType { get; set; } = string.Empty;

        // Ngày và thời gian chiếu
        public DateTime ShowDate { get; set; }
        public TimeSpan SlotStartTime { get; set; }
        public TimeSpan SlotEndTime { get; set; }
        public DateTime? ActualStartTime { get; set; }
        public DateTime? ActualEndTime { get; set; }
        public string Status { get; set; }

        // Thông tin bảng giá
        public string PricingTier { get; set; } = string.Empty;
        public decimal Multiplier { get; set; }

        // Thông tin thống kê
        public long TotalBookings { get; set; }
        public decimal? AvgTicketPrice { get; set; }
    }

}
