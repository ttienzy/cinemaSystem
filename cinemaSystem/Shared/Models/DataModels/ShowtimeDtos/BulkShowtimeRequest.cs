using System;
using System.Collections.Generic;

namespace Shared.Models.DataModels.ShowtimeDtos
{
    /// <summary>
    /// Request DTO for bulk creating showtimes.
    /// </summary>
    public class BulkShowtimeRequest
    {
        public Guid MovieId { get; set; }
        public Guid CinemaId { get; set; }
        public Guid ScreenId { get; set; }
        public Guid PricingTierId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        /// <summary>
        /// List of time slot definitions for bulk creation.
        /// </summary>
        public List<ShowtimeTimeSlotRequest> TimeSlots { get; set; } = new();

        /// <summary>
        /// Dates to exclude from scheduling (e.g., maintenance days).
        /// </summary>
        public List<DateTime> ExcludeDates { get; set; } = new();

        /// <summary>
        /// Pricing for each seat type.
        /// </summary>
        public List<ShowtimePricingInfoRequest> ShowtimePricings { get; set; } = new();
    }

    /// <summary>
    /// Time slot definition for bulk creation.
    /// </summary>
    public class ShowtimeTimeSlotRequest
    {
        /// <summary>
        /// TimeSlot ID to use for this showtime.
        /// </summary>
        public Guid SlotId { get; set; }

        /// <summary>
        /// Optional: override start time (HH:mm format). If not provided, uses TimeSlot.StartTime.
        /// </summary>
        public string? TimeOverride { get; set; }

        /// <summary>
        /// Days of week to apply (1=Monday, 7=Sunday). If null or empty, applies to all days.
        /// </summary>
        public List<int>? DaysOfWeek { get; set; }
    }

    /// <summary>
    /// Response DTO for bulk create result.
    /// </summary>
    public class BulkShowtimeResult
    {
        public int TotalCreated { get; set; }
        public int TotalSkipped { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<ShowtimeCreatedResult> CreatedShowtimes { get; set; } = new();
    }

    public class ShowtimeCreatedResult
    {
        public Guid ShowtimeId { get; set; }
        public DateTime ShowDate { get; set; }
        public string StartTime { get; set; } = string.Empty;
    }
}
