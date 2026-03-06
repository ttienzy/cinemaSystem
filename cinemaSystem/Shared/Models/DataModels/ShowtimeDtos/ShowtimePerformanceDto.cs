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

        // Movie Information
        public string Title { get; set; } = string.Empty;

        // Screen Information
        public string ScreenName { get; set; } = string.Empty;
        public string ScreenType { get; set; } = string.Empty;

        // Date and Time Information
        public DateTime ShowDate { get; set; }
        public TimeSpan SlotStartTime { get; set; }
        public TimeSpan SlotEndTime { get; set; }
        public DateTime? ActualStartTime { get; set; }
        public DateTime? ActualEndTime { get; set; }
        public string Status { get; set; }

        // Pricing Information
        public string PricingTier { get; set; } = string.Empty;
        public decimal Multiplier { get; set; }

        // Statistics Information
        public long TotalBookings { get; set; }
        public decimal? AvgTicketPrice { get; set; }
    }

}
